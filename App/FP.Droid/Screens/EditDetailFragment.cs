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
using System.Threading.Tasks;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens
{
	public class EditDetailFragment : Fragment
	{
		private Presentation currentEditDetail = null;
		private View contentView;
		private LayoutInflater inflater;
		private View viewEditDetail;
		private ScrollView svInfo;
		private LinearLayout llEditDetail;

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

				GoogleIO2012Helper helper = new GoogleIO2012Helper();

				Button btnSave = (Button)viewEditDetail.FindViewById(Resource.Id.btnSave);
				btnSave.Click += BtnSave_GloogleIO2012Slides_Click;

				Button btnRender = (Button)viewEditDetail.FindViewById(Resource.Id.btnRender);
				btnRender.Click += BtnRender_GoogleIO2012Slides_Click;


				Button btnPresent = (Button)viewEditDetail.FindViewById(Resource.Id.btnPresent);
				btnPresent.Click += BtnPresent_GloogleIO2012Slides_Click;

				// Präsentations Content laden und anzeigen
				EditText etContent = (EditText)viewEditDetail.FindViewById(Resource.Id.etContent);
				etContent.SetSingleLine(false);
				etContent.Text = helper.LoadContent(presentation.PresentationUID);

				// Die Anzeige zurücksetzen
				EditText etTitle = (EditText)viewEditDetail.FindViewById(Resource.Id.etTitle);
				etTitle.Text = String.Empty;

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


		public void Reset()
		{
			// Bereits vorhandene Details entfernen
			llEditDetail.RemoveAllViews();

			viewEditDetail = null;
		}

		void BtnRender_GoogleIO2012Slides_Click (object sender, EventArgs e)
		{
			// Async Daten Asyncron rendern lassen
			TaskScheduler context = TaskScheduler.FromCurrentSynchronizationContext();
			ProgressDialog pdlg = new ProgressDialog(Activity);
			pdlg.SetCancelable(false);
			pdlg.SetTitle(GetText(Resource.String.ProgressRenderPresentation));
			pdlg.SetMessage(GetText(Resource.String.PleaseWait));
			pdlg.Show();

			Task.Factory.StartNew(() => {


				return new WSRenderGoogleIO2012().RenderPresentation(currentEditDetail.PresentationUID);
			}).ContinueWith(t => {
				pdlg.Cancel();
				
				if (t.Exception == null && t.Result)
					Toast.MakeText(Activity, Resource.String.ToastPresentationRendered, ToastLength.Long).Show();
				else
				{
					Activity.RunOnUiThread(delegate() {
						AlertDialog adlg = new AlertDialog.Builder(Activity).Create();
						adlg.SetTitle(GetText(Resource.String.Error));
						adlg.SetMessage(GetText(Resource.String.ToastErrorRenderPresentation));
						adlg.Show();
					});
				}
			}, context);
		}

		private void BtnPresent_GloogleIO2012Slides_Click (object sender, EventArgs e)
		{
			StartPresentation(currentEditDetail.PresentationUID);
		}

		private void BtnSave_GloogleIO2012Slides_Click (object sender, EventArgs e)
		{
			if (currentEditDetail != null)
			{
				SavePresentation(currentEditDetail);
				Toast.MakeText(Activity, Resource.String.ToastPresentationSaved, ToastLength.Long).Show();
			}
		}

		public bool SavePresentation(Presentation presentation)
		{
			if (presentation == null || viewEditDetail == null)
			{
				// TODO Fehlermeldung
				return false;
			}
			
			// View für die entsprechenden Typen laden
			switch(presentation.Type)
			{
			case Presentation.Typ.GoogleIO2012Slides:
				GoogleIO2012Helper helper = new GoogleIO2012Helper();
				
				EditText etContent = (EditText)viewEditDetail.FindViewById(Resource.Id.etContent);
				if (!helper.SaveContent(presentation.PresentationUID, etContent.Text.Trim()))
				{
					// ToDo Fehlermeldung
				}

				// Die vorhande Konfiguration laden um nur geänderte Stellen zu überschreiben
				GoogleIO2012Config cfg = helper.LoadConfig(presentation.PresentationUID);

				if (cfg.settings != null)
				{
					GoogleIO2012ConfigSettings settings = cfg.settings;

					settings.title = ((EditText)viewEditDetail.FindViewById(Resource.Id.etTitle)).Text;
					settings.subtitle = ((EditText)viewEditDetail.FindViewById(Resource.Id.etSubTitle)).Text;
					settings.useBuilds = ((ToggleButton)viewEditDetail.FindViewById(Resource.Id.tbtnAnimation)).Checked;
					settings.enableSlideAreas = ((ToggleButton)viewEditDetail.FindViewById(Resource.Id.tbtnAreas)).Checked;
					settings.enableTouch = ((ToggleButton)viewEditDetail.FindViewById(Resource.Id.tbtnTouch)).Checked;
				}

				if (cfg.presenters != null && cfg.presenters.Count > 0)
				{
					GoogleIO2012ConfigPresenters pres = cfg.presenters.FirstOrDefault();

					pres.name = ((EditText)viewEditDetail.FindViewById(Resource.Id.etName)).Text;
					pres.company = ((EditText)viewEditDetail.FindViewById(Resource.Id.etCompany)).Text;
					pres.gplus = ((EditText)viewEditDetail.FindViewById(Resource.Id.etGooglePlus)).Text;
					pres.twitter = ((EditText)viewEditDetail.FindViewById(Resource.Id.etTwitter)).Text;
					pres.www = ((EditText)viewEditDetail.FindViewById(Resource.Id.etWebsite)).Text;
					pres.github = ((EditText)viewEditDetail.FindViewById(Resource.Id.etGithub)).Text;
				}

				helper.SaveConfig(presentation.PresentationUID, cfg);
				
				break;
			}


			return false;
		}

		private void StartPresentation(Guid presentationUID)
		{
			Intent intent = new Intent(Activity, typeof(BrowserActivity));
			string pFolder = Path.Combine(new PresentationsHelper().PresentationsFolder, presentationUID.ToString());			
			string demo = pFolder + "/template.html";
			
			intent.PutExtra("url", "file://" + demo);
			
			StartActivity(intent);
		}
	}
}

