/*
 * Copyright (C) 2012 James Montemagno (motz2k1@oh.rr.com) http://www.montemagno.com
 * 
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */

using System;
using Android.Runtime;
using Android.Views;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.AndroidHelper.AdMob
{
	public static class AdMobHelper
	{
		//this is where we had specified: admob6sample.admob;
		static IntPtr _helperClass = JNIEnv.FindClass("de/dhoffmann/mono/fullscreenpresentation/droid/androidhelper/admob/JAdMobHelper");

		/// <summary>
		/// Refreshed the ad for the view
		/// </summary>
		/// <param name="view"></param>
		public static void LoadAd(View view)
		{
			IntPtr methodId = JNIEnv.GetStaticMethodID(_helperClass, "loadAd", "(Landroid/view/View;)V");
			JNIEnv.CallStaticVoidMethod(_helperClass, methodId, new JValue(view));
		}
		
		/// <summary>
		/// Destroys the ad
		/// </summary>
		/// <param name="view"></param>
		public static void DestroyAd(View view)
		{
			IntPtr methodId = JNIEnv.GetStaticMethodID(_helperClass, "destroy", "(Landroid/view/View;)V");
			JNIEnv.CallStaticVoidMethod(_helperClass, methodId, new JValue(view));
		}
	}
	
}