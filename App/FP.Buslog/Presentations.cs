using System;
using System.IO;
using System.Collections.Generic;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Buslog
{
	public class Presentations
	{
		private string presentationsFolder = "Presentations";

		public enum ErrorCode : int
		{
			OK,
			ERROR,
			PRESENTATIONEXISTS
		}

		public Presentations ()
		{
#if MONODROID
			string path = System.Environment.SpecialFolder.Personal.ToString();
#endif
			// Den Order für die Presentationen festlegen
			presentationsFolder = Path.Combine(path, presentationsFolder);

			// Falls es den Ordner nicht gibt, dann anlegen
			if (!Directory.Exists(presentationsFolder))
				Directory.CreateDirectory(presentationsFolder);
		}

		public ErrorCode CreateNewPresentation(string name)
		{

			return ErrorCode.OK;
		}

		public Dictionary<Guid, string> GetPresentationList()
		{
			Dictionary<Guid, string> ret = new Dictionary<Guid, string>();

			return ret;
		}
	}
}

