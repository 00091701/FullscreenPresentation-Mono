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
using System.Collections.Generic;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Buslog
{
	public class GoogleIO2012Config
	{
		public string Title { get; set; }
		public string SubTitle { get; set; }
		public bool SlideAnimation { get; set; }
		public bool Prettify { get; set; }
		public bool SlideAreas { get; set; }
		public bool Touch { get; set; }
		public string AnalyticsKey { get; set; }
		public string Favicon {	get; set; }
		public List<string> Fonts { get; set; }
		public string Theme { get; set; }

		public List<GoogleIO2012ConfigPresenters> Presenters { get; set; }
	}
}