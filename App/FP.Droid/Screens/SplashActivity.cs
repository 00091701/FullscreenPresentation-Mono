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


using Android.App;
using Android.OS;
using De.Dhoffmann.Mono.FullscreenPresentation.Buslog;
using System.Threading.Tasks;
using System;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens
{
	[Activity (Label = "@string/app_name", MainLauncher = true, NoHistory = true, Theme = "@style/Theme.Splash", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape)]
	public class SplashActivity : BaseActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			AppInit appInit = new AppInit(this);
			appInit.AppStartAsync ().ContinueWith (t => {
				if (!String.IsNullOrEmpty(t.Result))
				{
					// Fehlermeldung anzeigen
					ShowErrorMsg(t.Result);
				}
				else
				{
					// und weiter gehts
					StartActivity(typeof(EditActivity));
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ());
		}
	}
}

