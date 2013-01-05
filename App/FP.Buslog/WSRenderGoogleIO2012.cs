/*
 * This file is part of Fullscreen-Presentation
 * Copyright (C) 2013 David Hoffmann
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
using System.Net;
using System.Text;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Buslog
{
	public class WSRenderGoogleIO2012
	{
		public WSRenderGoogleIO2012()
		{}

		public bool RenderPresentation(Guid presentationUID)
		{
			bool ret = false;
			string content = null;

#if MONODROID
			string proxyHost = Android.Net.Proxy.DefaultHost;
			int proxyPort = Android.Net.Proxy.DefaultPort;
#endif
			// Dateipfad zusammenbauen
			PresentationsHelper presentationHelper = new PresentationsHelper();
			
			string contentFolder = Path.Combine(presentationHelper.PresentationsFolder, presentationUID.ToString());
			contentFolder = Path.Combine(contentFolder, "scripts");
			contentFolder = Path.Combine(contentFolder, "md");

			// load base.html
			string baseFilename = Path.Combine(contentFolder, "base.html");

			if (!File.Exists(baseFilename))
				return false;

			// laod slides.md
			string slidesFilename = Path.Combine(contentFolder, "slides.md");

			if (!File.Exists(slidesFilename))
				return false;

			string baseContent = null;
			string slidesContent = null;

			// Dateien laden
			TextReader trBase = new StreamReader(baseFilename);
			baseContent = trBase.ReadToEnd();
			trBase.Close();

			TextReader trSlides = new StreamReader(slidesFilename);
			slidesContent = trSlides.ReadToEnd();
			trSlides.Close();

			// WebRequest
			string url = "https://ws-fp-app.appspot.com/";

			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
			request.CookieContainer = new CookieContainer();
			request.Headers.Add(HttpRequestHeader.AcceptCharset, "utf-8");
			request.Headers.Add(HttpRequestHeader.AcceptLanguage, System.Threading.Thread.CurrentThread.CurrentUICulture.Name + "," + System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			//Wenn ein Proxy im System eingestellt ist, diesen auch nutzen
			if(!String.IsNullOrEmpty(proxyHost))
				request.Proxy = new WebProxy(proxyHost, proxyPort);

			// Postdaten senden
			request.Method = WebRequestMethods.Http.Post;
			request.ContentType = "application/x-www-form-urlencoded";
			
			string postParam = "base=" + System.Web.HttpUtility.UrlEncode(baseContent);
			postParam += "&slides=" + System.Web.HttpUtility.UrlEncode(slidesContent);

			byte[] byteArray = Encoding.UTF8.GetBytes(postParam);
			request.ContentLength = byteArray.Length;
			
			Stream reqStream = request.GetRequestStream();
			reqStream.Write(byteArray, 0, byteArray.Length);
			reqStream.Close();
			
			try
			{
				using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
				{
					if(response.StatusCode == HttpStatusCode.OK)
					{
						Encoding encoding = Encoding.UTF8;
						
						try
						{
							if (!String.IsNullOrEmpty(response.CharacterSet))
								encoding = Encoding.GetEncoding(response.CharacterSet);
						}
						catch(Exception ex)
						{
							Logging.Log(this, Logging.LoggingTypeError, "Unbekanntes Encoding", ex);
						}
			
						using (Stream resStream = response.GetResponseStream())
						{
							StreamReader reader = new StreamReader(resStream, encoding);
							content = reader.ReadToEnd();
						}
					}
				}
			}
			catch(WebException ex)
			{
				Logging.Log(this, Logging.LoggingTypeError, "Render Error", ex);
			}

			if (!String.IsNullOrEmpty(content))
			{
				TextWriter tw = new StreamWriter(Path.Combine(Path.Combine(new PresentationsHelper().PresentationsFolder, presentationUID.ToString()), "template.html"), false);
				tw.Write(content);
				tw.Flush();
				tw.Close();

				ret = true;
			}

			return ret;
		}
	}
}