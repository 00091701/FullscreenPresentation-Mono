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

namespace De.Dhoffmann.Mono.FullscreenPresentation.Buslog
{
	public class Logging
	{
		public static int LoggingTypeVerbose;
		public static int LoggingTypeDebug = 1;
		public static int LoggingTypeInfo = 2;
		public static int LoggingTypeWarn = 3;
		public static int LoggingTypeError = 4;


		public static void Log(object sender, int loggingType, string logMsg)
		{
			Log(sender, loggingType, logMsg, null);
		}


		public static void Log(object sender, int loggingType, string logMsg, Exception ex)
		{
#if DEBUG
			bool debug = true;
#else
			bool debug = false;
#endif

			string loggingMessage = "Logging type: ";

			switch (loggingType)
			{
				case 0:
					loggingMessage += "VERBOSE";
					break;
				case 1:
					loggingMessage += "DEBUG";
					break;
				case 2:
					loggingMessage += "INFO";
					break;
				case 3:
					loggingMessage += "WARN";
					break;
				case 4:
					loggingMessage += "ERROR";
					break;
			}


			if (sender != null)
				loggingMessage += " - Sender: " + sender.GetType().Name;

			if (logMsg != null)
				loggingMessage += " - LogMsg: " + logMsg;

			if (ex != null)
				loggingMessage += " - EXCEPTION: " + ex;

#if MONODROID
			if (!debug && loggingType <= LoggingTypeDebug)
				return;

			string tag = "ADFCNewsApp";

			if (loggingType == LoggingTypeWarn)
				Android.Util.Log.Verbose(tag, loggingMessage);
			else if (loggingType == LoggingTypeDebug)
				Android.Util.Log.Debug(tag, loggingMessage);
			else if (loggingType == LoggingTypeInfo)
				Android.Util.Log.Info(tag, loggingMessage);
			else if (loggingType == LoggingTypeWarn)
				Android.Util.Log.Warn(tag, loggingMessage);
			else if (loggingType == LoggingTypeError)
				Android.Util.Log.Error(tag, loggingMessage);
#endif
#if MONOTOUCH
			if (!debug && loggingType > LoggingTypeDebug)
				Console.WriteLine(loggingMessage);
			else
				System.Diagnostics.Debug.WriteLine(loggingMessage);
#endif
		}
	}
}

