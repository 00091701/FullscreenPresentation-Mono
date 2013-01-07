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

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens
{
	public class EditSlidesListFragment : Fragment
	{
		private enum PresentationsMenu : int
		{
			Start = 0,
			Create,
			Rename,
			Delete
		}

		private View contentView;
		private int selectedItemPosition;

		public EditSlidesListFragment()
		{ 
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			((EditActivity)Activity).FragEditSlidesList = this;

			contentView = inflater.Inflate(Resource.Layout.EditSlidesList, null);
			selectedItemPosition= -1;

			ListView lvSlides = contentView.FindViewById<ListView>(Resource.Id.lvSlides);
			lvSlides.ItemClick += HandleItemClick;
			RegisterForContextMenu(lvSlides);

			LoadSlidesList();

			return contentView;
		}

		private void LoadSlidesList(Guid? selectedItemUID = null)
		{
			DBPresentation dbPresentation = new DBPresentation();
			List<Presentation> presentations = dbPresentation.Select();
			
			ListView lvSlides = contentView.FindViewById<ListView>(Resource.Id.lvSlides);
			lvSlides.Adapter = new SlidesAdapter(Activity, presentations);
		}

		private void HandleItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			ListView lv = (ListView)sender;
			EditActivity activityEdit = Activity as EditActivity;
			
			if (activityEdit != null)
			{
				SlidesAdapter adapter = (SlidesAdapter)lv.Adapter;

				// Die ausgewählte Präsentation laden
				if (activityEdit.FragEditDetail != null)
					activityEdit.FragEditDetail.LoadPresentation(adapter.GetPresentation(lv.CheckedItemPosition));
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
			selectedItemPosition = ((AdapterView.AdapterContextMenuInfo)menuInfo).Position;

			menu.SetHeaderTitle(Resource.String.PresentationsMenuTitle);
			menu.Add(0, (int)PresentationsMenu.Start, 0, Resource.String.PresentationsMenuStart);
			menu.Add(0, (int)PresentationsMenu.Create, 1, Resource.String.PresentationsMenuCreate);
			menu.Add(0, (int)PresentationsMenu.Rename, 2, Resource.String.PresentationsMenuRename);
			menu.Add(0, (int)PresentationsMenu.Delete, 3, Resource.String.PresentationsMenuDelete);
		}


		public override bool OnContextItemSelected (IMenuItem item)
		{
			ListView lvSlides = contentView.FindViewById<ListView>(Resource.Id.lvSlides);
			Guid presentationUID = ((SlidesAdapter)lvSlides.Adapter).GetPresentation(selectedItemPosition).PresentationUID;

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
			}

			selectedItemPosition = -1;
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
					ShowErrorMsg(GetText(Resource.String.DlgNewPresentationErrorNoName));
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
					if (presentations.CreateNew(presentationUID, name) != PresentationsHelper.ErrorCode.OK)
					{
						// Fehlermeldung anzeigen
						ShowErrorMsg(GetText(Resource.String.DlgNewPresentationError));
					}
					else
					{
						LoadSlidesList(presentationUID);
					}
				}
				else
				{
					// Fehlermeldung anzeigen
					ShowErrorMsg(GetText(Resource.String.DlgNewPresentationErrorPraesExists));
				}
			});
			
			dialog.SetNegativeButton(GetText(Resource.String.Cancel), delegate { });
			
			dialog.Show();
		}

		private void RenamePresentation(Guid presentationUID)
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
					ShowErrorMsg(GetText(Resource.String.DlgNewPresentationErrorNoName));
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

				presentations.Rename(presentationUID, name);
				LoadSlidesList(presentationUID);
			});

			dialog.SetNegativeButton(GetText(Resource.String.Cancel), delegate { });
			
			dialog.Show();
		}

		private void DeletePresentation(Guid presentationUID)
		{
			if (new PresentationsHelper().Delete(presentationUID) == PresentationsHelper.ErrorCode.MINIMALPRESENTATIONS)
			{
				ShowErrorMsg(GetText(Resource.String.ErrorMinimalPresentationCount));
			}

			LoadSlidesList();
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

