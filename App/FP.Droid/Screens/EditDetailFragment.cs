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
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Libs.FP.Data.Types;
using De.Dhoffmann.Mono.FullscreenPresentation.Buslog;
using System.Threading.Tasks;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.AndroidHelper.AdMob;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens
{
	public class EditDetailFragment : Fragment
	{
		View m_AdView;
		Presentation currentEditDetail;
		View contentView;
		LayoutInflater inflater;
		View viewEditDetail;
		LinearLayout llEditDetail;
		Presentation.Typ currentType = Presentation.Typ.UNDEFINED;
		GoogleIO2012Helper googleHelper;

		static View viewEditDetailGoogleIO2012Slides = null;

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "OnCreateView()");

			EditActivity editActivity = ((EditActivity)Activity); 
			editActivity.FragEditDetail = this;
			this.inflater = inflater;

			contentView = inflater.Inflate (Resource.Layout.EditDetail, null);

			llEditDetail = (LinearLayout)contentView.FindViewById(Resource.Id.llEditDetail);

			googleHelper = new GoogleIO2012Helper((EditActivity)Activity);

			// PreLoader
			Task.Factory.StartNew (() => {
				Activity.RunOnUiThread(() => {
					Logging.Log (this, Logging.LoggingTypeDebug, "OnCreateView() PreLoad Editors async");
					View tmp = GoogleIO2012PresentationEditor;
					Logging.Log (this, Logging.LoggingTypeDebug, "OnCreateView() PreLoad Editors async (finished)");
				});
			});

			return contentView;
		}

		public override void OnResume ()
		{
			base.OnResume ();
			Logging.Log (this, Logging.LoggingTypeDebug, "OnResume()");

			ReloadAd();
		}
	
		public void LoadPresentation (Presentation presentation)
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "LoadPresentation()");

			if (presentation == null)
			{
				// TODO Fehlermeldung
				return;
			}

			currentEditDetail = presentation;

			// View für die entsprechenden Typen laden
			switch(presentation.Type)
			{
			case Presentation.Typ.GoogleIO2012Slides:
				SetHasOptionsMenu(true);

				viewEditDetail = GoogleIO2012PresentationEditor;
				googleHelper.LoadGoogleIO2012Presentation(viewEditDetail, presentation);
				break;

			default:
				throw new NotImplementedException();
			}

			// ... und anzeigen
			if (currentType != presentation.Type)
			{
				llEditDetail.RemoveAllViews();
				llEditDetail.AddView(viewEditDetail);
				currentType = presentation.Type;
			}
		}

		View GoogleIO2012PresentationEditor
		{
			get
			{
				if (viewEditDetailGoogleIO2012Slides == null)
				{
					Logging.Log (this, Logging.LoggingTypeDebug, "LoadGoogleIO2012PresentationEditor(): Load editor");
					viewEditDetailGoogleIO2012Slides = googleHelper.LoadGoogleIO2012PresentationEditor(viewEditDetail, inflater);
					Logging.Log (this, Logging.LoggingTypeDebug, "LoadGoogleIO2012PresentationEditor(): Load editor (finished)");
				}

				return viewEditDetailGoogleIO2012Slides;
			}
		}

		void ReloadAd()
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "ReloadAd()");

			if (viewEditDetail == null)
				return;

			View adWrapper = viewEditDetail.FindViewById<View> (Resource.Id.adWrapper);

			if (adWrapper != null)
			{
				if (!String.IsNullOrEmpty(GetText(Resource.String.AdPublisherID)))
				{
					adWrapper.Visibility = ViewStates.Visible;

					if (m_AdView == null && viewEditDetail != null)
						m_AdView = viewEditDetail.FindViewById(Resource.Id.adView);

					if (m_AdView != null)
					{
						Task.Factory.StartNew(() => {
							Activity.RunOnUiThread(delegate() {
								if (m_AdView != null)
									AdMobHelper.LoadAd(this.Activity, m_AdView);
							});
						});
					}
				}
				else
				{
					adWrapper.Visibility = ViewStates.Gone;
				}
			}
		}

		public override void OnCreateOptionsMenu (IMenu menu, MenuInflater inflater)
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "OnCreateOptionsMenu()");

			if (currentEditDetail != null)
			{
				Task.Factory.StartNew(() => {
					this.Activity.RunOnUiThread(() => {
						switch(currentEditDetail.Type)
						{
						case Presentation.Typ.GoogleIO2012Slides:
							menu = googleHelper.OnCreateOptionsMenu(menu, inflater);
							break;

						default:
							throw new NotImplementedException();
						}
					});
				});
			}

			base.OnCreateOptionsMenu (menu, inflater);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (currentEditDetail != null)
			{
				switch(currentEditDetail.Type)
				{
				case Presentation.Typ.GoogleIO2012Slides:
					item = googleHelper.OnOptionsItemSelected(item, this, viewEditDetail, currentEditDetail);
					break;

				default:
					throw new NotImplementedException();
				}
			}

			return base.OnOptionsItemSelected (item);
		}


		public override void OnDestroyView ()
		{
			if(m_AdView != null)
				AdMobHelper.DestroyAd(m_AdView);

			base.OnDestroyView ();
		}
	}
}

