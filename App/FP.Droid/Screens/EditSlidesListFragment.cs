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
		private View contentView;

		public EditSlidesListFragment()
		{ 
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{

			contentView = inflater.Inflate(Resource.Layout.EditSlidesList, null);

			DBPresentation dbPresentation = new DBPresentation();
			List<Presentation> presentations = dbPresentation.Select();

			ListView lvSlides = contentView.FindViewById<ListView>(Resource.Id.lvSlides);
			lvSlides.Adapter = new SlidesAdapter(Activity, presentations);

			return contentView;
		}
		/*

		private void BtnDemo_Click1 (object sender, EventArgs e)
		{
			Intent intent = new Intent(Activity, typeof(BrowserActivity));

			string pFolder = Path.Combine(new PresentationsHelper().PresentationsFolder, Guid.Empty.ToString());

			string demo = pFolder + "/template.html";

			intent.PutExtra("url", "file://" + demo);
			//intent.PutExtra("url", "http://io-2012-slides.googlecode.com/git/template.html");

			StartActivity(intent);
		}

		private void BtnNewPresentation_Click (object sender, EventArgs e)
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

				PresentationsHelper presentations;
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
					if (presentations.CreateNew(Guid.Empty, name) != PresentationsHelper.ErrorCode.OK)
					{
						// Fehlermeldung anzeigen
						ShowErrorMsg(GetText(Resource.String.DlgNewPresentationError));
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

*/
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

