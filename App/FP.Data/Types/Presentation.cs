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

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Libs.FP.Data.Types
{
	public class Presentation
	{
		public enum Typ : int
		{
			UNDEFINED = -1,
			GoogleIO2012Slides = 0
		}
	
		public Guid PresentationUID { get; set; }
		public string Name { get; set; }
		public DateTime DateCreate { get; set; }
		public Typ Type { get; set; }
	}
}

