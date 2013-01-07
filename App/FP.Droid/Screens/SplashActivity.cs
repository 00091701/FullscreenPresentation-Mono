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
using Android.Views;
using Android.Widget;
using De.Dhoffmann.Mono.FullscreenPresentation.Buslog;
using System.Threading.Tasks;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens
{
	[Activity (Label = "@string/app_name", MainLauncher = true, NoHistory = true, Theme = "@style/Theme.Splash", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape)]
	public class SplashActivity : Activity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// asynchron ist immer toll!
			TaskScheduler context = TaskScheduler.FromCurrentSynchronizationContext();
			
			Task.Factory.StartNew(() => {
				// Datenbank einrichten / aktualisiern
				AppInit appInit = new AppInit(this);
				return appInit.IsError;
			}).ContinueWith(t => {
				if (!(bool)t.Result)
				{
					// Ein bißchen Splashscreen ist immer erlaubt
					System.Threading.Thread.Sleep(1000);

					// und weiter gehts
					StartActivity(typeof(EditActivity));
				}
			}, context);
		}
	}
}

