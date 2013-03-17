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
using System.IO;
using System.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using Android.App;
using Android.Views;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Libs.FP.Data.Types;
using Android.Widget;
using System.Threading.Tasks;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens;
using System.Linq;
using Android.Content;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.AndroidHelper;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Buslog
{
	public class GoogleIO2012Helper
	{
		object context;
		PresentationsHelper presentationsHelper;

		EditText etContent;
		EditText etTitle;
		EditText etTitle2;
		EditText etSubTitle;
		ToggleButton tbtnAnimation;
		ToggleButton tbtnAreas;
		ToggleButton tbtnTouch;
		EditText etName;
		EditText etCompany;
		EditText etGooglePlus;
		EditText etTwitter;
		EditText etWebsite;
		EditText etGithub;

		public enum ActionBarButtons : int
		{
			SelectPresentation = 0,
			Save,
			Render,
			Present
		}

		public GoogleIO2012Helper(object context)
		{
			this.context = context;

			try
			{
				presentationsHelper = new PresentationsHelper(context);
			}
			catch (Exception)
			{
				((BaseActivity)context).ShowErrorMsg((context as Activity).GetText(Resource.String.ErrorNoExternalStorage));
			}

			presentationsHelper = new PresentationsHelper(context);
		}

		public IMenu OnCreateOptionsMenu (IMenu menu, MenuInflater inflate)
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "OnCreateOptionsMenu()");

			var m0 = menu.Add (0, (int)ActionBarButtons.SelectPresentation, 0, Resource.String.btnSelectPresentation);
			m0.SetShowAsAction (ShowAsAction.Always);

			var m1 = menu.Add (0, (int)ActionBarButtons.Save, 1, Resource.String.btnSave);
			m1.SetShowAsAction (ShowAsAction.Always);

			var m2 = menu.Add (0, (int)ActionBarButtons.Render, 2, Resource.String.btnRender);
			m2.SetShowAsAction (ShowAsAction.Always);

			var m3 = menu.Add (0, (int)ActionBarButtons.Present, 3, Resource.String.btnPresent);
			m3.SetShowAsAction (ShowAsAction.Always);

			return menu;
		}

		public IMenuItem OnOptionsItemSelected (IMenuItem item, EditDetailFragment fragment, View viewEditDetail, Presentation presentation)
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "OnOptionsItemSelected()");

			switch((ActionBarButtons)item.ItemId)
			{
			case ActionBarButtons.SelectPresentation:
				fragment.Activity.RunOnUiThread(() => {
					EditActivity editActivity = context as EditActivity;

					if (editActivity != null)
						editActivity.ShowPresentationSelection();

					fragment.SetHasOptionsMenu(false);
				});
				break;

			case ActionBarButtons.Save:
				if (presentation != null)
				{
					SavePresentation(fragment, viewEditDetail, presentation);
					Toast.MakeText(fragment.Activity, Resource.String.ToastPresentationSaved, ToastLength.Long).Show();
				}
				break;

			case ActionBarButtons.Render:
				// Async Daten Asyncron rendern lassen
				ProgressDialog pdlg = new ProgressDialog(fragment.Activity);
				pdlg.SetCancelable(false);
				pdlg.SetTitle(fragment.GetText(Resource.String.ProgressRenderPresentation));
				pdlg.SetMessage(fragment.GetText(Resource.String.PleaseWait));
				pdlg.Show();

				Task.Factory.StartNew(() => {
					return new WSRenderGoogleIO2012(this.context).RenderPresentation(presentation.PresentationUID);
				}).ContinueWith(t => {
					pdlg.Cancel();
					
					if (t.Exception == null && t.Result)
						Toast.MakeText(fragment.Activity, Resource.String.ToastPresentationRendered, ToastLength.Long).Show();
					else
					{
						fragment.Activity.RunOnUiThread(delegate() {
							((BaseActivity)fragment.Activity).ShowErrorMsg(fragment.GetText(Resource.String.ToastErrorRenderPresentation));
						});
					}
				}, TaskScheduler.FromCurrentSynchronizationContext());
				break;

			case ActionBarButtons.Present:
				StartPresentation(fragment, presentation);
				break;
			}

			return item;
		}


		public string LoadContent(Guid presentationUID)
		{
			string ret;

			// Dateinamen zusammenbauen
			string contentFileName = Path.Combine(presentationsHelper.PresentationsFolder, presentationUID.ToString());
			contentFileName = Path.Combine(contentFileName, "scripts");
			contentFileName = Path.Combine(contentFileName, "md");
			contentFileName = Path.Combine(contentFileName, "slides.md");

			if (!File.Exists(contentFileName))
				return null;

			// Datei auslesen
			TextReader txtReader = new StreamReader(contentFileName);
			ret = txtReader.ReadToEnd();
			txtReader.Close();

			return ret;
		}

		public bool SaveContent(Guid presentationUID, string content)
		{
			// Dateinamen zusammenbauen
			string contentFileName = Path.Combine(presentationsHelper.PresentationsFolder, presentationUID.ToString());
			contentFileName = Path.Combine(contentFileName, "scripts");
			contentFileName = Path.Combine(contentFileName, "md");
			contentFileName = Path.Combine(contentFileName, "slides.md");

			// Datei schreiben
			try
			{
				TextWriter txtWriter = new StreamWriter(contentFileName, false);
				txtWriter.Write(content);
				txtWriter.Flush();
				txtWriter.Close();

				return true;
			}
			catch (Exception ex)
			{
				Logging.Log(this, Logging.LoggingTypeError, "can't save presentation content", ex);
			}

			return false;
		}

		public GoogleIO2012Config LoadConfig(Guid presentationUID)
		{
			GoogleIO2012Config ret = new GoogleIO2012Config();

			// Dateinamen zusammenbauen
			string contentFileName = Path.Combine(presentationsHelper.PresentationsFolder, presentationUID.ToString());
			contentFileName = Path.Combine(contentFileName, "slide_config.js");
			
			if (!File.Exists(contentFileName))
				return ret;

			// Datei auslesen
			TextReader txtReader = new StreamReader(contentFileName);
			string sCfg = txtReader.ReadToEnd();
			txtReader.Close();

			// Den JavaScript Overhead entfernen
			sCfg = sCfg.Remove(0, sCfg.IndexOf('{'));
			sCfg = sCfg.Remove(sCfg.LastIndexOf('}') + 1, (sCfg.Length - (sCfg.LastIndexOf('}') + 1)));

			// Javascript Kommentare entfernen
			sCfg = Regex.Replace(sCfg, "[^:](//.*?)\n", "\n", RegexOptions.Multiline);
			sCfg = Regex.Replace(sCfg, @"/\*(.*?)\*/", "", RegexOptions.Singleline);

			// Json parsen
			JsonValue jsonCfg = JsonObject.Parse(sCfg);

			if (jsonCfg != null && jsonCfg.Count >= 0)
			{
				if (jsonCfg.ContainsKey("settings"))
				{
					ret.settings = new GoogleIO2012ConfigSettings();

					JsonValue settings = jsonCfg["settings"];

					if (settings.ContainsKey("title"))
						ret.settings.title = settings["title"];

					if (settings.ContainsKey("subtitle"))
						ret.settings.subtitle = settings["subtitle"];

					if (settings.ContainsKey("useBuilds"))
						ret.settings.useBuilds = settings["useBuilds"];

					if (settings.ContainsKey("usePrettify"))
						ret.settings.usePrettify = settings["usePrettify"];

					if (settings.ContainsKey("enableSlideAreas"))
						ret.settings.enableSlideAreas = settings["enableSlideAreas"];

					if (settings.ContainsKey("enableTouch"))
						ret.settings.enableTouch = settings["enableTouch"];

					if (settings.ContainsKey("analytics"))
						ret.settings.analytics = settings["analytics"];

					if (settings.ContainsKey("favIcon"))
						ret.settings.favIcon = settings["favIcon"];

					if (settings.ContainsKey("fonts"))
					{
						ret.settings.fonts = new List<string>();

						foreach(JsonValue font in settings["fonts"] as JsonArray)
							ret.settings.fonts.Add(font.ToString().Substring(1, font.ToString().Length - 2));
					}

					if (settings.ContainsKey("theme"))
						ret.settings.theme = settings["theme"];
				}

				if (jsonCfg.ContainsKey("presenters"))
				{
					ret.presenters = new List<GoogleIO2012ConfigPresenters>();
					JsonValue presenters = jsonCfg["presenters"];

					foreach(JsonValue presenter in presenters as JsonArray)
					{
						GoogleIO2012ConfigPresenters pres = new GoogleIO2012ConfigPresenters();

						if (presenter.ContainsKey("name"))
							pres.name = presenter["name"];
						
						if (presenter.ContainsKey("company"))
							pres.company = presenter["company"];
						
						if (presenter.ContainsKey("gplus"))
							pres.gplus = presenter["gplus"];
						
						if (presenter.ContainsKey("twitter"))
							pres.twitter = presenter["twitter"];
						
						if (presenter.ContainsKey("www"))
							pres.www = presenter["www"];

						if (presenter.ContainsKey("github"))
							pres.github = presenter["github"];

						ret.presenters.Add(pres);
					}
				}
			}

			return ret;
		}

		public bool SaveConfig(Guid presentationUID, GoogleIO2012Config config)
		{
			// Json erstellen
			string sCfg = "var SLIDE_CONFIG = ";

			var serializer = new DataContractJsonSerializer(typeof(GoogleIO2012Config));
			using (var stream = new MemoryStream())
			{
				serializer.WriteObject(stream, config);
				sCfg += System.Text.Encoding.Default.GetString(stream.ToArray());
			}

			sCfg += ";";

			// Format korrigieren
			sCfg = sCfg.Replace("\"__type\":\"GoogleIO2012ConfigPresenters:#De.Dhoffmann.Mono.FullscreenPresentation.Buslog\",", "");
			sCfg = sCfg.Replace("\\/", "/");

			// Dateinamen zusammenbauen
			string settingsFileName = Path.Combine(presentationsHelper.PresentationsFolder, presentationUID.ToString());
			settingsFileName = Path.Combine(settingsFileName, "slide_config.js");
			
			// Datei schreiben
			try
			{
				TextWriter txtWriter = new StreamWriter(settingsFileName, false);
				txtWriter.Write(sCfg);
				txtWriter.Flush();
				txtWriter.Close();
				
				return true;
			}
			catch (Exception ex)
			{
				Logging.Log(this, Logging.LoggingTypeError, "can't save presentation settings", ex);
			}

			return false;
		}

		public bool SavePresentation(EditDetailFragment fragment, View viewEditDetail, Presentation presentation)
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
				GoogleIO2012Helper helper = new GoogleIO2012Helper(context);

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

					string title2 = ((EditText)viewEditDetail.FindViewById(Resource.Id.etTitle2)).Text;

					if (!String.IsNullOrEmpty(title2))
						settings.title += "<br />" + title2;

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
		
		public void StartPresentation(EditDetailFragment fragment, Presentation presentation)
		{
			Intent intent = new Intent(fragment.Activity, typeof(BrowserActivity));
			string pFolder = Path.Combine(presentationsHelper.PresentationsFolder, presentation.PresentationUID.ToString());			
			string demo = pFolder + "/template.html";
			
			intent.PutExtra("url", "file://" + demo);
			
			fragment.StartActivity(intent);
		}

		public void CreatePresentation(Presentation presentation)
		{
			Activity activity = this.context as Activity;

			// Per Dialog den Namen der neuen Presentation abfragen
			AlertDialog.Builder dialog = new AlertDialog.Builder(activity);
			dialog.SetTitle(activity.GetText(Resource.String.DlgNewPresentationTitle));
			dialog.SetMessage(activity.GetText(Resource.String.DlgNewPresentationText));
			dialog.SetCancelable(true);

			EditText etName = new EditText(activity);
			etName.SetSingleLine(true);

			dialog.SetView(etName);
			dialog.SetPositiveButton(activity.GetText(Resource.String.DlgNewPresentationErstellen), delegate {
				string name = etName.Text.Trim();
				Guid newPresentationUID;

				if (String.IsNullOrEmpty(name))
				{
					// Fehlermeldung anzeigen
					((BaseActivity)activity).ShowErrorMsg(activity.GetText(Resource.String.DlgPresentationErrorNoName));
				}

				// Gibt es die Präsentation schon?
				if (!presentationsHelper.Exists(name))
				{
					// Präsentation erstellen
					if (presentationsHelper.CreateNew(presentation.PresentationUID, out newPresentationUID, name) != PresentationsHelper.ErrorCode.OK)
					{
						// Fehlermeldung anzeigen
						((BaseActivity)activity).ShowErrorMsg(activity.GetText(Resource.String.DlgNewPresentationError));
					}
				}
				else
				{
					// Fehlermeldung anzeigen
					((BaseActivity)activity).ShowErrorMsg(activity.GetText(Resource.String.DlgPresentationErrorPraesExists));
				}

				// Präsentation laden
				if (this.context.GetType() == typeof(EditActivity))
				{
					EditActivity editActivity = this.context as EditActivity;
					presentation.PresentationUID = newPresentationUID;
					editActivity.FragEditDetail.LoadPresentation(presentation);
					editActivity.HidePresentationSelection();
				}
			});

			dialog.SetNegativeButton(activity.GetText(Resource.String.Cancel), delegate { });
			dialog.Show();

		}

		public void RenamePresentation(Presentation presentation)
		{
			Activity activity = this.context as Activity;

			// Per Dialog den Namen der neuen Presentation abfragen
			AlertDialog.Builder dialog = new AlertDialog.Builder(activity);
			dialog.SetTitle(activity.GetText(Resource.String.DlgRenamePresentationTitle));
			dialog.SetMessage(activity.GetText(Resource.String.DlgRenamePresentationText));
			dialog.SetCancelable(true);

			EditText etName = new EditText(activity);
			etName.SetSingleLine(true);

			dialog.SetView(etName);
			dialog.SetPositiveButton(activity.GetText(Resource.String.DlgRenamePresentationErstellen), delegate {
				string name = etName.Text.Trim();

				if (String.IsNullOrEmpty(name))
				{
					// Fehlermeldung anzeigen
					((BaseActivity)activity).ShowErrorMsg(activity.GetText(Resource.String.DlgPresentationErrorNoName));
				}

				try
				{
					presentationsHelper = new PresentationsHelper(this.context);
				}
				catch (Exception)
				{
					((BaseActivity)activity).ShowErrorMsg(activity.GetText(Resource.String.ErrorNoExternalStorage));
				}

				presentationsHelper.Rename(presentation.PresentationUID, name);
				ListView lvSlides = activity.FindViewById<ListView>(Resource.Id.lvSlides);

				foreach (Presentation p in (((SlidesAdapter)lvSlides.Adapter).GetData))
				{
					if (p.PresentationUID == presentation.PresentationUID)
					{
						p.Name = name;
						break;
					}
				}

				// Liste aktualisieren
				if (this.context.GetType() == typeof(EditActivity))
				{
					EditActivity editActivity = this.context as EditActivity;
					editActivity.LoadSlidesList();
				}
			});

			dialog.SetNegativeButton(activity.GetText(Resource.String.Cancel), delegate { });
			dialog.Show();
		}

		public void DeletePresentation(Presentation presentation)
		{
			if (presentationsHelper.Delete(presentation.PresentationUID) == PresentationsHelper.ErrorCode.MINIMALPRESENTATIONS)
			{
				Activity activity = this.context as Activity;
				((BaseActivity)activity).ShowErrorMsg(activity.GetText(Resource.String.ErrorMinimalPresentationCount));
			}
			else
			{
				// Liste aktualisieren
				if (this.context.GetType() == typeof(EditActivity))
				{
					EditActivity editActivity = this.context as EditActivity;
					editActivity.LoadSlidesList();
				}
			}
		}

		public void ShowPresentationsFolder(Presentation presentation)
		{
			string folder = Path.Combine(presentationsHelper.PresentationsFolder, presentation.PresentationUID.ToString());
			(this.context as BaseActivity).ShowErrorMsg(folder, (this.context as Activity).GetText(Resource.String.DlgTitleShowPresentationsFolder));
		}

		public View LoadGoogleIO2012PresentationEditor(View viewEditDetail, LayoutInflater inflater)
		{
			if (viewEditDetail == null)
			{
				Logging.Log (this, Logging.LoggingTypeDebug, "LoadGoogleIO2012PresentationEditor() - inflate");
				viewEditDetail = inflater.Inflate(Resource.Layout.EditDetailGoogleIO2012, null);
				Logging.Log (this, Logging.LoggingTypeDebug, "LoadGoogleIO2012PresentationEditor() - inflate (finished)");
			}

			return viewEditDetail;
		}

		public void LoadGoogleIO2012Presentation(View viewEditDetail, Presentation presentation)
		{
			// Präsentations Content laden und anzeigen
			etContent = (EditText)viewEditDetail.FindViewById(Resource.Id.etContent);
			etContent.SetSingleLine(false);
			
			etTitle = (EditText)viewEditDetail.FindViewById(Resource.Id.etTitle);
			etTitle2 = (EditText)viewEditDetail.FindViewById(Resource.Id.etTitle2);
			etSubTitle = (EditText)viewEditDetail.FindViewById(Resource.Id.etSubTitle);
			tbtnAnimation = (ToggleButton)viewEditDetail.FindViewById(Resource.Id.tbtnAnimation);
			tbtnAreas = (ToggleButton)viewEditDetail.FindViewById(Resource.Id.tbtnAreas);
			tbtnTouch = (ToggleButton)viewEditDetail.FindViewById(Resource.Id.tbtnTouch);
			etName = (EditText)viewEditDetail.FindViewById(Resource.Id.etName);
			etCompany = (EditText)viewEditDetail.FindViewById(Resource.Id.etCompany);
			etGooglePlus = (EditText)viewEditDetail.FindViewById(Resource.Id.etGooglePlus);
			etTwitter = (EditText)viewEditDetail.FindViewById(Resource.Id.etTwitter);
			etWebsite = (EditText)viewEditDetail.FindViewById(Resource.Id.etWebsite);
			etGithub = (EditText)viewEditDetail.FindViewById(Resource.Id.etGithub);

			etContent.Text = LoadContent(presentation.PresentationUID);

			// Die Anzeige zurücksetzen
			etTitle.Text = String.Empty;
			etTitle2.Text = String.Empty;
			etSubTitle.Text = String.Empty;
			tbtnAnimation.Checked = false;
			tbtnAreas.Checked = false;
			tbtnTouch.Checked = false;
			etName.Text = String.Empty;
			etCompany.Text = String.Empty;
			etGooglePlus.Text = String.Empty;
			etTwitter.Text = String.Empty;
			etWebsite.Text = String.Empty;
			etGithub.Text = String.Empty;

			// Konfiguration laden und anzeigen
			GoogleIO2012Config config = LoadConfig(presentation.PresentationUID);
			
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
		}
	}
}