/*
 * This file is part of Fullscreen-Presentation
 * Copyright (C) 2013 David Hoffmann
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

using Android.App;
using Android.Views.InputMethods;
using Android.Content;
using Android.Views;


namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens
{
	[Activity]
	public abstract class BaseActivity : Activity
	{
		public void ShowErrorMsg(string errMsg, string title = null)
		{
			if (string.IsNullOrEmpty (title))
				title = GetText (Resource.String.ErrMsgTitle);

			// Fehlermeldung anzeigen
			AlertDialog.Builder alert = new AlertDialog.Builder(this);
			alert.SetTitle(title);
			alert.SetMessage(errMsg);
			alert.SetCancelable(true);
			alert.SetPositiveButton(GetText(Resource.String.Ok), delegate { });
			alert.Show();
		}
	}
}
