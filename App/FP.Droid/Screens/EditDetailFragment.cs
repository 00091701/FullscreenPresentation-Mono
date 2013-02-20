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
		ScrollView svInfo;
		LinearLayout llEditDetail;

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			((EditActivity)Activity).FragEditDetail = this;
			this.inflater = inflater;
			contentView = inflater.Inflate(Resource.Layout.EditDetail, null);

			llEditDetail = (LinearLayout)contentView.FindViewById(Resource.Id.llEditDetail);

			// Info einblenden
			svInfo = (ScrollView)contentView.FindViewById(Resource.Id.svInfo);
			svInfo.Visibility = ViewStates.Visible;

			return contentView;
		}

		public override void OnResume ()
		{
			base.OnResume ();

			ReloadAd();
		}

		public override void OnPause ()
		{
			base.OnPause ();
		}
	
		public void LoadPresentation (Presentation presentation)
		{
			if (presentation == null)
			{
				// TODO Fehlermeldung
				return;
			}

			currentEditDetail = presentation;

			// Info ausblenden
			svInfo.Visibility = ViewStates.Gone;

			// Bereits vorhandene Details entfernen
			Reset();

			// View für die entsprechenden Typen laden
			switch(presentation.Type)
			{
			case Presentation.Typ.GoogleIO2012Slides:
				if (viewEditDetail == null)
					viewEditDetail = inflater.Inflate(Resource.Layout.EditDetailGoogleIO2012, null);

				SetHasOptionsMenu(true);
				Activity.InvalidateOptionsMenu();

				GoogleIO2012Helper helper = new GoogleIO2012Helper();
				ReloadAd();

				// Präsentations Content laden und anzeigen
				EditText etContent = (EditText)viewEditDetail.FindViewById(Resource.Id.etContent);
				etContent.SetSingleLine(false);
				etContent.Text = helper.LoadContent(presentation.PresentationUID);

				// Die Anzeige zurücksetzen
				EditText etTitle = (EditText)viewEditDetail.FindViewById(Resource.Id.etTitle);
				etTitle.Text = String.Empty;
				EditText etTitle2 = (EditText)viewEditDetail.FindViewById(Resource.Id.etTitle2);
				etTitle2.Text = String.Empty;

				EditText etSubTitle = (EditText)viewEditDetail.FindViewById(Resource.Id.etSubTitle);
				etSubTitle.Text = String.Empty;

				ToggleButton tbtnAnimation = (ToggleButton)viewEditDetail.FindViewById(Resource.Id.tbtnAnimation);
				tbtnAnimation.Checked = false;

				ToggleButton tbtnAreas = (ToggleButton)viewEditDetail.FindViewById(Resource.Id.tbtnAreas);
				tbtnAreas.Checked = false;

				ToggleButton tbtnTouch = (ToggleButton)viewEditDetail.FindViewById(Resource.Id.tbtnTouch);
				tbtnTouch.Checked = false;

				EditText etName = (EditText)viewEditDetail.FindViewById(Resource.Id.etName);
				etName.Text = String.Empty;

				EditText etCompany = (EditText)viewEditDetail.FindViewById(Resource.Id.etCompany);
				etCompany.Text = String.Empty;

				EditText etGooglePlus = (EditText)viewEditDetail.FindViewById(Resource.Id.etGooglePlus);
				etGooglePlus.Text = String.Empty;

				EditText etTwitter = (EditText)viewEditDetail.FindViewById(Resource.Id.etTwitter);
				etTwitter.Text = String.Empty;

				EditText etWebsite = (EditText)viewEditDetail.FindViewById(Resource.Id.etWebsite);
				etWebsite.Text = String.Empty;

				EditText etGithub = (EditText)viewEditDetail.FindViewById(Resource.Id.etGithub);
				etGithub.Text = String.Empty;

				// Konfiguration laden und anzeigen
				GoogleIO2012Config config = helper.LoadConfig(presentation.PresentationUID);

				if (config != null)
				{
					if (config.settings != null)
					{
						GoogleIO2012ConfigSettings settings = config.settings;

						etTitle.Text = settings.title;

						if (settings.title.Contains("<br />"))
						{
							int nIndex = settings.title.IndexOf("<br />", StringComparison.InvariantCulture);
							etTitle.Text = settings.title.Substring(0, nIndex).Trim();
							etTitle2.Text = settings.title.Substring(nIndex+6, settings.title.Length-nIndex-6).Trim();
						}

						etSubTitle.Text = settings.subtitle;
						tbtnAnimation.Checked = settings.useBuilds;
						tbtnAreas.Checked = settings.enableSlideAreas;
						tbtnTouch.Checked = settings.enableTouch;
					}

					if (config.presenters != null && config.presenters.Count > 0)
					{
						// Das UI unterstützt derzeit nur einen Presenter
						GoogleIO2012ConfigPresenters presenter = config.presenters.FirstOrDefault();

						if (presenter != null)
						{
							etName.Text = presenter.name;
							etCompany.Text = presenter.company;
							etGooglePlus.Text = presenter.gplus;
							etTwitter.Text = presenter.twitter;
							etWebsite.Text = presenter.www;
							etGithub.Text = presenter.github;
						}
					}
				}

				break;
			}

			// ... und anzeigen
			llEditDetail.AddView(viewEditDetail);
		}

		void ReloadAd()
		{
			if (m_AdView == null && viewEditDetail != null)
				m_AdView = viewEditDetail.FindViewById(Resource.Id.adView);

			Task.Factory.StartNew(() => {
				Activity.RunOnUiThread(delegate() {
					if (m_AdView != null)
						AdMobHelper.LoadAd(m_AdView);
				});
			});
		}

		public void Reset()
		{
			// Buttons entfernen
			SetHasOptionsMenu(false);
			Activity.InvalidateOptionsMenu();

			// Bereits vorhandene Details entfernen
			llEditDetail.RemoveAllViews();

			viewEditDetail = null;
		}

		public override void OnCreateOptionsMenu (IMenu menu, MenuInflater inflater)
		{
			if (currentEditDetail != null)
			{
				switch(currentEditDetail.Type)
				{
				case Presentation.Typ.GoogleIO2012Slides:
					menu = new GoogleIO2012Helper().OnCreateOptionsMenu(menu, inflater);
					break;
				}
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
					item = new GoogleIO2012Helper().OnOptionsItemSelected(item, this, viewEditDetail, currentEditDetail);
					break;
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

