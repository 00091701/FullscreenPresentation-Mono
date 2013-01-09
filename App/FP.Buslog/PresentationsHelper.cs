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
using System.Collections.Generic;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Libs.FP.Data.Types;
using De.Dhoffmann.Mono.FullscreenPresentation.Data;
using System.Linq;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Buslog
{
	public class PresentationsHelper
	{
		private string presentationsFolder = "Fullscreen-Presentations";

		public enum ErrorCode : int
		{
			OK,
			ERROR,
			PRESENTATIONEXISTS,
			MINIMALPRESENTATIONS
		}

		public PresentationsHelper ()
		{
			if (Android.OS.Environment.ExternalStorageState != "mounted")
				throw new Exception("no external storage");


#if MONODROID
			string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
#endif

			// Den Order für die Presentationen festlegen
			PresentationsFolder = Path.Combine(path, PresentationsFolder);

			// Falls es den Ordner nicht gibt, dann anlegen
			if (!Directory.Exists(PresentationsFolder))
				Directory.CreateDirectory(PresentationsFolder);
		}

		public bool Exists (string name)
		{
			bool ret = false;

			return ret;
		}

		public ErrorCode CreateNew(Guid templateUID, out Guid newPresentationUID, string name)
		{
			newPresentationUID = Guid.Empty;

			if (templateUID == Guid.Empty || String.IsNullOrEmpty(name))
				return ErrorCode.ERROR;

			// Template aus der DB laden
			DBPresentation dbPresentation = new DBPresentation();
			List<Presentation> templates = dbPresentation.Select(templateUID);

			if (templates == null || templates.Count == 0)
				return ErrorCode.ERROR;

			Presentation template = templates.FirstOrDefault();

			// copy files
			newPresentationUID = Guid.NewGuid();

			CopyDirectory(new DirectoryInfo(Path.Combine(PresentationsFolder, templateUID.ToString())), new DirectoryInfo(Path.Combine(PresentationsFolder, newPresentationUID.ToString())));

			return CreateNew(newPresentationUID, name, template.Type);
		}

		public ErrorCode Rename(Guid presentationUID, string name)
		{
			DBPresentation dbPresentation = new DBPresentation();
			Presentation presentation = dbPresentation.Select(presentationUID).FirstOrDefault();
			presentation.Name = name;
			if (dbPresentation.Update(presentation))
				return ErrorCode.OK;
			else 
				return ErrorCode.ERROR;
		}

		public ErrorCode CreateNew(Guid presentationUID, string name, Presentation.Typ typ)
		{
			if (presentationUID == Guid.Empty || String.IsNullOrEmpty(name))
				return ErrorCode.ERROR;

			DBPresentation dbPresentation = new DBPresentation();

			if (dbPresentation.Insert(new Presentation() {
				PresentationUID = presentationUID,
				Name = name,
				Type = typ
			}))
				return ErrorCode.OK;
			else
				return ErrorCode.ERROR;
		}

		public ErrorCode Delete(Guid presentationUID)
		{
			DBPresentation dbPresentation = new DBPresentation();

			List<Presentation> presentations = dbPresentation.Select(null);
			if (presentations.Count <= 1)
				return ErrorCode.MINIMALPRESENTATIONS;

			if (dbPresentation.Delete(presentationUID))
			{
				string path = Path.Combine(presentationsFolder, presentationUID.ToString());

				if (Directory.Exists(path))
					Directory.Delete(path, true);

				return ErrorCode.OK;
			}

			return ErrorCode.ERROR;
		}


		public Dictionary<Guid, string> GetPresentationList()
		{
			Dictionary<Guid, string> ret = new Dictionary<Guid, string>();

			return ret;
		}

		public string PresentationsFolder
		{
			get { return presentationsFolder; }
			private set { presentationsFolder = value; }
		}

		public void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
		{
			// Check if the target directory exists, if not, create it.
			if (Directory.Exists(target.FullName) == false)
				Directory.CreateDirectory(target.FullName);

			// Copy each file into it’s new directory.
			foreach (FileInfo fi in source.GetFiles())
				fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
				CopyDirectory(diSourceSubDir, nextTargetSubDir);
			}
		}
	}
}

