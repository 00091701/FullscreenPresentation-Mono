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
using System.Collections.Generic;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Buslog
{
	public class GoogleIO2012ConfigSettings
	{
		public string title { get; set; }
		public string subtitle { get; set; }
		public bool useBuilds { get; set; }
		public bool usePrettify { get; set; }
		public bool enableSlideAreas { get; set; }
		public bool enableTouch { get; set; }
		public string analytics { get; set; }
		public string favIcon {	get; set; }
		public List<string> fonts { get; set; }
		public string theme { get; set; }
	}
}