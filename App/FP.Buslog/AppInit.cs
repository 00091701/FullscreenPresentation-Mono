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
using De.Dhoffmann.Mono.FullscreenPresentation.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Libs.FP.Data.Types;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Buslog
{
	public class AppInit
	{
		public bool IsError { get; set; }

		public AppInit (object context)
		{
			IsError = false;

			// Datenbank anlegen / updaten
			new DBSchema().UpdateDBSchema();

#if MONODROID
			Android.App.Activity activity = (Android.App.Activity)context;
#endif

			PresentationsHelper presentationsHelper = null;
			try
			{
				presentationsHelper = new PresentationsHelper();
			}
			catch (Exception ex)
			{
				Logging.Log(this, Logging.LoggingTypeError, "Fehler beim instanziieren des PresentationsHelper()", ex);
#if MONODROID
				ShowErrorMsg(context, activity.GetText(De.Dhoffmann.Mono.FullscreenPresentation.Droid.Resource.String.ErrorNoExternalStorage));
				return;
#endif
			}

			DirectoryInfo dirInfo = new DirectoryInfo(presentationsHelper.PresentationsFolder);

			List<Presentation> presentations = new DBPresentation().Select(null);

			// Nicht registrierte Präsentationen löschen
			foreach (DirectoryInfo dir in dirInfo.GetDirectories())
			{
				if (presentations.Count(p => Path.Combine(presentationsHelper.PresentationsFolder, p.PresentationUID.ToString()) == dir.ToString()) == 0)
				{
					// Kann das Verzeichnis als Präsentation erkannt und importiert werden?
					Guid presentationUID;

					if (Guid.TryParse(dir.Name, out presentationUID))
					{
						// Ist es vielleicht eine "GoogleIO2012" Präsentation?
						if(File.Exists(Path.Combine(dir.ToString(), "slide_config.js")))
						{
							// Scheint so
							// Wird also als GoogleIO2012 importiert
							GoogleIO2012Helper helper = new GoogleIO2012Helper();
							GoogleIO2012Config config =  helper.LoadConfig(presentationUID);

							if (config.settings != null && !String.IsNullOrEmpty(config.settings.title))
							{
								presentationsHelper.CreateNew(presentationUID, config.settings.title, Presentation.Typ.GoogleIO2012Slides);
								presentations.Add(new Presentation {
									PresentationUID = presentationUID,
									Name = config.settings.title,
									DateCreate = DateTime.Now,
									Type = Presentation.Typ.GoogleIO2012Slides
								});
							}
							else
								dir.Delete(true);
						}
						else
						{
							// Ist doch ein unbekannter Typ
							dir.Delete(true);
						}
					}
					else
					{
						// Es ist wohl keine Präsentation
						dir.Delete(true);
					}
				}
			}

			// Gelöschte Präsentationen aus der Datenbank entfernen
			foreach(Presentation pres in presentations)
			{
				if (!Directory.Exists(Path.Combine(presentationsHelper.PresentationsFolder, pres.PresentationUID.ToString())))
					new DBPresentation().Delete(pres.PresentationUID);
			}
	
			// Wenn es noch keine Präsentation gibt, die mitgelieferte als Demo / Vorlage kopieren
			if (dirInfo.GetDirectories().Length == 0)
			{
				Guid pUID = Guid.NewGuid();
				string pFolder = Path.Combine(presentationsHelper.PresentationsFolder, pUID.ToString());

				try
				{
					Directory.CreateDirectory(pFolder);

					Dictionary<string, List<string>> dictTemplates = new Dictionary<string, List<string>>();
					dictTemplates.Add("io-2012-slides", new List<string> {
						"images/barchart.png",
						"images/chart.png",
						"images/chrome-logo-tiny.png",
						"images/google_developers_icon_128.png",
						"images/google_developers_logo.png",
						"images/google_developers_logo_tiny.png",
						"images/google_developers_logo_white.png",
						"images/io2012_logo.png",
						"images/sky.jpg",

						"js/polyfills/classList.min.js",
						"js/polyfills/dataset.min.js",
						"js/polyfills/history.min.js",

						"js/prettify/lang-apollo.js",
						"js/prettify/lang-clj.js",
						"js/prettify/lang-css.js",
						"js/prettify/lang-go.js",
						"js/prettify/lang-hs.js",
						"js/prettify/lang-lisp.js",
						"js/prettify/lang-lua.js",
						"js/prettify/lang-ml.js",
						"js/prettify/lang-n.js",
						"js/prettify/lang-proto.js",
						"js/prettify/lang-scala.js",
						"js/prettify/lang-sql.js",
						"js/prettify/lang-tex.js",
						"js/prettify/lang-vb.js",
						"js/prettify/lang-vhdl.js",
						"js/prettify/lang-wiki.js",
						"js/prettify/lang-xq.js",
						"js/prettify/lang-yaml.js",
						"js/prettify/prettify.css",
						"js/prettify/prettify.js",

						"js/hammer.js",
						"js/modernizr.custom.45394.js",
						"js/order.js",
						"js/require-1.0.8.min.js",
						"js/slide-controller.js",
						"js/slide-deck.js",
						"js/slides.js",

						"scripts/md/base.html",
						"scripts/md/README.md",
						"scripts/md/render.py",
						"scripts/md/slides.md",

						"theme/css/default.css",
						"theme/css/phone.css",

						"theme/scss/_base.scss",
						"theme/scss/default.scss",
						"theme/scss/phone.scss",

						"app.yaml",
						"config.rb",
						"README.html",
						"README.md",
						"serve.sh",
						"slide_config.js",
						"template.html"
					});

					string filename = null;
					foreach (KeyValuePair<string, List<string>> kvpTemplate in dictTemplates)
					{
						foreach (string f in kvpTemplate.Value)
						{
							filename = Path.Combine(pFolder, f);

							using (Stream stream = activity.Assets.Open("FP.Assets/" + kvpTemplate.Key + "/" + f))
							{ 
								if (f.Contains("/"))
								{
									int nIndex = filename.LastIndexOf('/');
									string dirName = filename.Substring(0, nIndex);
									Directory.CreateDirectory(dirName);
								}

								Stream swOut = new FileStream(filename, FileMode.CreateNew);
								byte[] buffer = new byte[1024];
								int b = buffer.Length;
								int length;

								while ((length = stream.Read(buffer, 0, b)) > 0)
								{
									swOut.Write(buffer, 0, length);
								}
								
								swOut.Flush();
								swOut.Close();
								stream.Close();
							}
						}

						// Die Präsentation in der DB registrieren
						string name = null;
						Presentation.Typ typ;
						switch(kvpTemplate.Key)
						{
						case "io-2012-slides":
							typ = Presentation.Typ.GoogleIO2012Slides;
							name = ((Android.App.Activity)context).GetText(De.Dhoffmann.Mono.FullscreenPresentation.Droid.Resource.String.PresentationTyp_GoogleIO2012Slides);
							break;
						default:
							throw new Exception("Unknown presentation type");
						}

						presentationsHelper.CreateNew(pUID, name, typ);
					}
				}
				catch(Exception)
				{
					// Wenn etwas schief läuft den Ordner wieder aufräumen
					if (Directory.Exists(presentationsHelper.PresentationsFolder))
						Directory.Delete(presentationsHelper.PresentationsFolder, true);

					ShowErrorMsg(context, activity.GetText(De.Dhoffmann.Mono.FullscreenPresentation.Droid.Resource.String.DlgNewPresentationError));
				}
			}
		}

		void ShowErrorMsg(object context, string errMsg)
		{
			IsError = true;
#if MONODROID
			Android.App.Activity activity = (Android.App.Activity)context;

			// Fehlermeldung anzeigen
			Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(activity);
			alert.SetTitle(activity.GetText(De.Dhoffmann.Mono.FullscreenPresentation.Droid.Resource.String.ErrMsgTitle));
			alert.SetMessage(errMsg);
			alert.SetCancelable(true);
			alert.SetPositiveButton(activity.GetText(De.Dhoffmann.Mono.FullscreenPresentation.Droid.Resource.String.Ok), delegate { });
			alert.Show();
#endif
		}
	}
}

