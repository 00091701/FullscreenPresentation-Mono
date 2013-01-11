/*
 * This file is part of Fullscreen-Presentation
 * Copyright (C) 2012 David Hoffmann
 *
 * Fullscreen-Presentation is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, version 2.
 *
 * Fullscreen-Presentation is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AFullscreen-Presentation; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 *
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using De.Dhoffmann.Mono.FullscreenPresentation.Buslog;
using Android.Views.InputMethods;
using Android.InputMethodServices;
using System.IO;
using De.Dhoffmann.Mono.FullscreenPresentation.Data;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Libs.FP.Data.Types;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.AndroidHelper;
using System.Threading.Tasks;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens
{
	public class EditSlidesListFragment : Fragment
	{
		private enum PresentationsMenu : int
		{
			Start = 0,
			Create,
			Rename,
			Delete,
			ShowFolder
		}

		private View contentView;
		private int selectedClickItemPosition;
		private int selectedLongClickItemPosition;
		private View currentItem = null;

		private ListView lvSlides;

		public EditSlidesListFragment()
		{ 
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			((EditActivity)Activity).FragEditSlidesList = this;

			contentView = inflater.Inflate(Resource.Layout.EditSlidesList, null);
			selectedClickItemPosition = -1;
			selectedLongClickItemPosition= -1;

			lvSlides = contentView.FindViewById<ListView>(Resource.Id.lvSlides);
			lvSlides.ItemClick += HandleItemClick;
			RegisterForContextMenu(lvSlides);

			LoadSlidesList();

			return contentView;
		}

		private void LoadSlidesList(Guid? selectedItemUID = null)
		{
			List<Presentation> presentations = null;

			// Wenn kein bestimmtes Element ausgewählt wurde,
			// die aktuelle Auswahl erhalten
			if (!selectedItemUID.HasValue)
			{
				if (selectedClickItemPosition >= 0)
				{
					presentations = ((SlidesAdapter)lvSlides.Adapter).GetData;
					selectedItemUID = presentations[selectedClickItemPosition].PresentationUID;
				}
			}

			DBPresentation dbPresentation = new DBPresentation();
			presentations = dbPresentation.Select();

			lvSlides.Adapter = new SlidesAdapter(Activity, presentations, SelectedItemBound, selectedItemUID);
		}

		private void SelectedItemBound()
		{
			currentItem = ((SlidesAdapter)lvSlides.Adapter).SelectedItem;
		}

		private void HandleItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			EditActivity activityEdit = Activity as EditActivity;

			selectedClickItemPosition = lvSlides.CheckedItemPosition;

			// Ist schon eine ausgewählt?
			if (currentItem != null)
				currentItem.SetBackgroundColor(Android.Graphics.Color.Black);

			// Ausgewählte Zeile markieren.
			currentItem = lvSlides.GetChildAt(selectedClickItemPosition);
			if (currentItem != null)
				currentItem.SetBackgroundColor(Android.Graphics.Color.Rgb(49, 182, 231));

			if (activityEdit != null)
			{
				SlidesAdapter adapter = (SlidesAdapter)lvSlides.Adapter;
				Presentation curPres = adapter.GetPresentation(lvSlides.CheckedItemPosition);
				adapter.SelectedItemUID = curPres.PresentationUID;

				// Die ausgewählte Präsentation laden
				if (activityEdit.FragEditDetail != null)
				{
					Task.Factory.StartNew(() => {
						Activity.RunOnUiThread(delegate() {
							activityEdit.FragEditDetail.LoadPresentation(curPres);
						});
					});
				}
				else
				{
					AlertDialog adlg = new AlertDialog.Builder(Activity).Create();
					adlg.SetTitle(GetText(Resource.String.Error));
					adlg.SetMessage(GetText(Resource.String.OnlyLandscape));
					adlg.Show();
				}
				
			}
		}

		public override void OnCreateContextMenu (IContextMenu menu, Android.Views.View v, IContextMenuContextMenuInfo menuInfo)
		{
			base.OnCreateContextMenu (menu, v, menuInfo);
			selectedLongClickItemPosition = ((AdapterView.AdapterContextMenuInfo)menuInfo).Position;

			menu.SetHeaderTitle(Resource.String.PresentationsMenuTitle);
			menu.Add(0, (int)PresentationsMenu.Start, 0, Resource.String.PresentationsMenuStart);
			menu.Add(0, (int)PresentationsMenu.Create, 1, Resource.String.PresentationsMenuCreate);
			menu.Add(0, (int)PresentationsMenu.Rename, 2, Resource.String.PresentationsMenuRename);
			menu.Add(0, (int)PresentationsMenu.Delete, 3, Resource.String.PresentationsMenuDelete);
			menu.Add(0, (int)PresentationsMenu.ShowFolder, 4, Resource.String.PresentationsMenuShowFolder);
		}


		public override bool OnContextItemSelected (IMenuItem item)
		{
			Guid presentationUID = ((SlidesAdapter)lvSlides.Adapter).GetPresentation(selectedLongClickItemPosition).PresentationUID;

			switch((PresentationsMenu)item.ItemId)
			{
			case PresentationsMenu.Start:
				StartPresentation(presentationUID);
				break;
			case PresentationsMenu.Create:
				CreatePresentation(presentationUID);
				break;
			case PresentationsMenu.Rename:
				RenamePresentation(presentationUID);
				break;
			case PresentationsMenu.Delete:
				DeletePresentation(presentationUID);
				break;
			case PresentationsMenu.ShowFolder:
				ShowPresentationsFolder(presentationUID);
				break;
			}

			selectedLongClickItemPosition = -1;

			return base.OnContextItemSelected (item);
		}
	

		private void StartPresentation(Guid presentationUID)
		{
			Intent intent = new Intent(Activity, typeof(BrowserActivity));
			string pFolder = Path.Combine(new PresentationsHelper().PresentationsFolder, presentationUID.ToString());			
			string demo = pFolder + "/template.html";
			
			intent.PutExtra("url", "file://" + demo);

			StartActivity(intent);
		}
	
		private void CreatePresentation(Guid presentationUID)
		{
			// Per Dialog den Namen der neuen Presentation abfragen
			AlertDialog.Builder dialog = new AlertDialog.Builder(Activity);
			dialog.SetTitle(GetText(Resource.String.DlgNewPresentationTitle));
			dialog.SetMessage(GetText(Resource.String.DlgNewPresentationText));
			dialog.SetCancelable(true);
			
			EditText etName = new EditText(Activity);
			etName.SetSingleLine(true);
			
			dialog.SetView(etName);
			
			dialog.SetPositiveButton(GetText(Resource.String.DlgNewPresentationErstellen), delegate {
				string name = etName.Text.Trim();
				
				if (String.IsNullOrEmpty(name))
				{
					// Fehlermeldung anzeigen
					ShowErrorMsg(GetText(Resource.String.DlgPresentationErrorNoName));
				}
				
				PresentationsHelper presentations = null;
				try
				{
					presentations = new PresentationsHelper();
				}
				catch (Exception)
				{
					ShowErrorMsg(GetText(Resource.String.ErrorNoExternalStorage));
				}
				
				// Gibt es die Präsentation schon?
				if (!presentations.Exists(name))
				{
					// Präsentation erstellen
					Guid newPresentationUID = Guid.Empty;

					if (presentations.CreateNew(presentationUID, out newPresentationUID, name) != PresentationsHelper.ErrorCode.OK)
					{
						// Fehlermeldung anzeigen
						ShowErrorMsg(GetText(Resource.String.DlgNewPresentationError));
					}
					else
					{
						LoadSlidesList(newPresentationUID);

						int pos = 0;
						foreach(Presentation p in ((SlidesAdapter)lvSlides.Adapter).GetData)
						{
							if (p.PresentationUID == newPresentationUID)
							{
								lvSlides.PerformItemClick(lvSlides, pos, lvSlides.GetItemIdAtPosition(pos)); 
								break;
							}

							pos++;
						}
					}
				}
				else
				{
					// Fehlermeldung anzeigen
					ShowErrorMsg(GetText(Resource.String.DlgPresentationErrorPraesExists));
				}
			});
			
			dialog.SetNegativeButton(GetText(Resource.String.Cancel), delegate { });
			
			dialog.Show();
		}

		private void RenamePresentation(Guid presentationUID)
		{
			// Per Dialog den Namen der neuen Presentation abfragen
			AlertDialog.Builder dialog = new AlertDialog.Builder(Activity);
			dialog.SetTitle(GetText(Resource.String.DlgRenamePresentationTitle));
			dialog.SetMessage(GetText(Resource.String.DlgRenamePresentationText));
			dialog.SetCancelable(true);
			
			EditText etName = new EditText(Activity);
			etName.SetSingleLine(true);
			
			dialog.SetView(etName);
			
			dialog.SetPositiveButton(GetText(Resource.String.DlgRenamePresentationErstellen), delegate {
				string name = etName.Text.Trim();
				
				if (String.IsNullOrEmpty(name))
				{
					// Fehlermeldung anzeigen
					ShowErrorMsg(GetText(Resource.String.DlgPresentationErrorNoName));
				}
				
				PresentationsHelper presentationsHelper = null;
				try
				{
					presentationsHelper = new PresentationsHelper();
				}
				catch (Exception)
				{
					ShowErrorMsg(GetText(Resource.String.ErrorNoExternalStorage));
				}

				presentationsHelper.Rename(presentationUID, name);

				ListView lvSlides = contentView.FindViewById<ListView>(Resource.Id.lvSlides);

				foreach (Presentation p in ((List<Presentation>)((SlidesAdapter)lvSlides.Adapter).GetData))
				{
					if (p.PresentationUID == presentationUID)
					{
						p.Name = name;
						break;
					}
				}
			});

			dialog.SetNegativeButton(GetText(Resource.String.Cancel), delegate { });
			
			dialog.Show();
		}

		private void DeletePresentation(Guid presentationUID)
		{
			Guid? selectedItemUID = ((SlidesAdapter)lvSlides.Adapter).GetData[selectedClickItemPosition].PresentationUID;

			if (new PresentationsHelper().Delete(presentationUID) == PresentationsHelper.ErrorCode.MINIMALPRESENTATIONS)
			{
				ShowErrorMsg(GetText(Resource.String.ErrorMinimalPresentationCount));
			}


			LoadSlidesList(selectedItemUID);

			// Inhalt entfernen
			if (selectedClickItemPosition == selectedLongClickItemPosition)
			{
				if (((EditActivity)Activity).FragEditDetail != null)
				{
					((EditActivity)Activity).FragEditDetail.Reset();
					currentItem = null;
					selectedClickItemPosition = -1;
				}
			}
		}

		private void ShowPresentationsFolder(Guid presentationUID)
		{
			string folder = Path.Combine(new PresentationsHelper().PresentationsFolder, presentationUID.ToString());

			// Fehlermeldung anzeigen
			AlertDialog.Builder alert = new AlertDialog.Builder(Activity);
			alert.SetTitle(GetText(Resource.String.DlgTitleShowPresentationsFolder));
			alert.SetMessage(folder);
			alert.SetCancelable(true);
			alert.SetPositiveButton(GetText(Resource.String.Ok), delegate { });
			alert.Show();
		}

		private void ShowErrorMsg(string errMsg)
		{
			// Tastatur ausblenden
			InputMethodManager imm = (InputMethodManager)Activity.GetSystemService (Context.InputMethodService);
			imm.HideSoftInputFromWindow (View.WindowToken, 0);

			// Fehlermeldung anzeigen
			AlertDialog.Builder alert = new AlertDialog.Builder(Activity);
			alert.SetTitle(GetText(Resource.String.ErrMsgTitle));
			alert.SetMessage(errMsg);
			alert.SetCancelable(true);
			alert.SetPositiveButton(GetText(Resource.String.Ok), delegate { });
			alert.Show();
		}
	}
}

