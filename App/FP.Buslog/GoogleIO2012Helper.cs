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

namespace De.Dhoffmann.Mono.FullscreenPresentation.Buslog
{
	public class GoogleIO2012Helper
	{
		public GoogleIO2012Helper()
		{}

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
			sCfg = Regex.Replace(sCfg, @"(\t|\n|\r)*", "", RegexOptions.Singleline);

			// Json parsen
			JsonValue jsonCfg = JsonObject.Parse(sCfg);


			return ret;
		}

		public bool SaveConfig(GoogleIO2012Config config)
		{
			string sCfg = "var SLIDE_CONFIG = ";

				sCfg += ";";
			return false;
		}
	}
}