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
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Libs.FP.Data.Types;
using De.Dhoffmann.Mono.FullscreenPresentation.Buslog;
using System.IO;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens
{
	public class EditDetailFragment : Fragment
	{
		private Presentation currentEditDetail = null;
		private View contentView;
		private LayoutInflater inflater;

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			((EditActivity)Activity).FragEditDetail = this;
			this.inflater = inflater;
			contentView = inflater.Inflate(Resource.Layout.EditDetail, null);

			((LinearLayout)contentView.FindViewById(Resource.Id.llPresentationFolder)).Visibility = ViewStates.Gone;

			ImageButton btnOpenFolder = (ImageButton)contentView.FindViewById(Resource.Id.btnOpenFolder);
			btnOpenFolder.Click += BtnOpenFolder_Click;

			return contentView;
		}

		void BtnOpenFolder_Click (object sender, EventArgs e)
		{
			string uri = ((TextView)contentView.FindViewById(Resource.Id.tvPresentationPath)).Text;

			// TODO - Funktioniert nocht nicht so richtig
			Intent intent = new Intent(Intent.ActionView);
			intent.SetType("application/*");
			intent.SetData(Android.Net.Uri.Parse("file://" + uri));

			try
			{
				StartActivity(intent);
			}
			catch(Exception)
			{
				// Fehlermeldung anzeigen
				AlertDialog.Builder alert = new AlertDialog.Builder(Activity);
				alert.SetTitle(GetText(Resource.String.ErrMsgTitle));
				alert.SetMessage(Resource.String.ErrMsgCouldNotOpenFileManager);
				alert.SetCancelable(true);
				alert.SetPositiveButton(GetText(Resource.String.Ok), delegate { });
				alert.Show();
			}

		}

		public void LoadPresentation (Presentation presentation)
		{
			if (presentation == null)
			{
				// TODO Fehlermeldung
				return;
			}

			currentEditDetail = presentation;
			((LinearLayout)contentView.FindViewById(Resource.Id.llPresentationFolder)).Visibility = ViewStates.Visible;
			PresentationsHelper presentationsHelper = new PresentationsHelper();

			// Pfad anzeigen
			((TextView)contentView.FindViewById(Resource.Id.tvPresentationPath)).Text = Path.Combine(presentationsHelper.PresentationsFolder, presentation.PresentationUID.ToString()).ToString();

			LinearLayout llEditDetail = (LinearLayout)contentView.FindViewById(Resource.Id.llEditDetail);

			// Bereits vorhandene Details entfernen
			llEditDetail.RemoveAllViews();

			View viewEditDetail = null;

			// View für die entsprechenden Typen laden
			switch(presentation.Type)
			{
			case Presentation.Typ.GoogleIO2012Slides:
				viewEditDetail = inflater.Inflate(Resource.Layout.EditDetailGoogleIO2012, null);
				break;
			}

			// ... und anzeigen
			llEditDetail.AddView(viewEditDetail);
		}
	}
}

