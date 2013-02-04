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
using Android.Webkit;
using De.Dhoffmann.Mono.FullscreenPresentation.Buslog;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens
{
	[Activity (Label = "BrowserActivity", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape)]
	public class BrowserActivity : Activity
	{
		private WebView webView;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// set fullscreen
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen | WindowManagerFlags.LayoutInScreen);
			Window.DecorView.SystemUiVisibility = StatusBarVisibility.Hidden;

			SetContentView(Resource.Layout.Browser);

			// load presentation
			webView = FindViewById<WebView>(Resource.Id.webView);
			webView.Settings.JavaScriptEnabled = true;
			webView.Settings.BuiltInZoomControls = false;
			
			webView.SetWebViewClient(new MyWebViewClient(Window));

			string url = Intent.GetStringExtra("url");
			
			// Präsentation anzeigen
			webView.LoadUrl(url);
		}

		public override bool OnKeyUp (Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Back)
			{
				webView.ClearCache(true);
				this.Finish();
			}

			return base.OnKeyUp (keyCode, e);
		}

		// don't use default browser
		class MyWebViewClient : WebViewClient
		{
			Window wnd;

			public MyWebViewClient(Window wnd)
			{
				this.wnd = wnd;
			}

			public override void DoUpdateVisitedHistory (WebView view, string url, bool isReload)
			{
				base.DoUpdateVisitedHistory (view, url, isReload);

				// restore fullscreen
				this.wnd.DecorView.SystemUiVisibility = StatusBarVisibility.Hidden;
			}


			public override bool ShouldOverrideUrlLoading (WebView view, string url)
			{
				return true;
			}
		}


		public override bool OnTouchEvent (MotionEvent e)
		{
			// restore fullscreen
			Window.DecorView.SystemUiVisibility = StatusBarVisibility.Hidden;
			
			return base.OnTouchEvent (e);
		}
	}
}

