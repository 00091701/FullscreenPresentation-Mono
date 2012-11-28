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


			Button btnDemo1 = contentView.FindViewById<Button>(Resource.Id.btnDemo1);
			btnDemo1.Click += BtnDemo_Click1;

			Button btnNewPresentation = contentView.FindViewById<Button>(Resource.Id.btnNewPresentation);
			btnNewPresentation.Click += BtnNewPresentation_Click;

			return contentView;
		}


		private void BtnDemo_Click1 (object sender, EventArgs e)
		{
			Intent intent = new Intent(Activity, typeof(BrowserActivity));
			intent.PutExtra("url", "http://io-2012-slides.googlecode.com/git/template.html");

			StartActivity(intent);
		}

		private void BtnNewPresentation_Click (object sender, EventArgs e)
		{
			// Per Dialog den Namen der neuen Presentation abfragen

		}


	}
}

