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

namespace De.Dhoffmann.Mono.FullscreenPresentation.Buslog
{
	public class GoogleIO2012Helper
	{
		public enum ActionBarButtons : int
		{
			Save = 0,
			Render,
			Present
		}

		public GoogleIO2012Helper()
		{}

		public IMenu OnCreateOptionsMenu (IMenu menu, MenuInflater inflate)
		{
			var m1 = menu.Add(0, (int)ActionBarButtons.Save, 0, Resource.String.btnSave);
			m1.SetShowAsAction(ShowAsAction.Always);

			var m2 = menu.Add(0, (int)ActionBarButtons.Render, 1, Resource.String.btnRender);
			m2.SetShowAsAction(ShowAsAction.Always);

			var m3 = menu.Add(0, (int)ActionBarButtons.Present, 2, Resource.String.btnPresent);
			m3.SetShowAsAction(ShowAsAction.Always);

			return menu;
		}

		public IMenuItem OnOptionsItemSelected (IMenuItem item, EditDetailFragment fragment, View viewEditDetail, Presentation presentation)
		{
			switch((ActionBarButtons)item.ItemId)
			{
			case ActionBarButtons.Save:
				if (presentation != null)
				{
					SavePresentation(fragment, viewEditDetail, presentation);
					Toast.MakeText(fragment.Activity, Resource.String.ToastPresentationSaved, ToastLength.Long).Show();
				}
				break;

			case ActionBarButtons.Render:
				// Async Daten Asyncron rendern lassen
				TaskScheduler context = TaskScheduler.FromCurrentSynchronizationContext();
				ProgressDialog pdlg = new ProgressDialog(fragment.Activity);
				pdlg.SetCancelable(false);
				pdlg.SetTitle(fragment.GetText(Resource.String.ProgressRenderPresentation));
				pdlg.SetMessage(fragment.GetText(Resource.String.PleaseWait));
				pdlg.Show();
				
				Task.Factory.StartNew(() => {
					
					
					return new WSRenderGoogleIO2012().RenderPresentation(presentation.PresentationUID);
				}).ContinueWith(t => {
					pdlg.Cancel();
					
					if (t.Exception == null && t.Result)
						Toast.MakeText(fragment.Activity, Resource.String.ToastPresentationRendered, ToastLength.Long).Show();
					else
					{
						fragment.Activity.RunOnUiThread(delegate() {
							AlertDialog adlg = new AlertDialog.Builder(fragment.Activity).Create();
							adlg.SetTitle(fragment.GetText(Resource.String.Error));
							adlg.SetMessage(fragment.GetText(Resource.String.ToastErrorRenderPresentation));
							adlg.Show();
						});
					}
				}, context);
				break;

			case ActionBarButtons.Present:
				StartPresentation(fragment, presentation.PresentationUID);
				break;
			}

			return item;
		}


		public string LoadContent(Guid presentationUID)
		{
			string ret = null;

			// Dateinamen zusammenbauen
			PresentationsHelper presentationHelper = new PresentationsHelper();

			string contentFileName = Path.Combine(presentationHelper.PresentationsFolder, presentationUID.ToString());
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
			PresentationsHelper presentationHelper = new PresentationsHelper();
			
			string contentFileName = Path.Combine(presentationHelper.PresentationsFolder, presentationUID.ToString());
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
			PresentationsHelper presentationHelper = new PresentationsHelper();
			
			string contentFileName = Path.Combine(presentationHelper.PresentationsFolder, presentationUID.ToString());
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
			PresentationsHelper presentationHelper = new PresentationsHelper();
			
			string settingsFileName = Path.Combine(presentationHelper.PresentationsFolder, presentationUID.ToString());
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
		
		private void StartPresentation(EditDetailFragment fragment, Guid presentationUID)
		{
			Intent intent = new Intent(fragment.Activity, typeof(BrowserActivity));
			string pFolder = Path.Combine(new PresentationsHelper().PresentationsFolder, presentationUID.ToString());			
			string demo = pFolder + "/template.html";
			
			intent.PutExtra("url", "file://" + demo);
			
			fragment.StartActivity(intent);
		}

	}
}