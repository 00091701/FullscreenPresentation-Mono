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
using Android.Widget;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Libs.FP.Data.Types;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using System.Linq;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.AndroidHelper
{

	public class SlidesAdapter : BaseAdapter
	{
		private Activity context;
		private List<Presentation> presentations;
		public  Guid? SelectedItemUID { get; set; }
		public View SelectedItem { get; private set; }
		private Action SelectedItemBound;

		public SlidesAdapter (Activity context, List<Presentation> presentations, Action SelectedItemBound, Guid? selectedItemUID = null)
		{
			this.context = context;
			this.presentations = presentations;
			this.SelectedItemUID = selectedItemUID;
			this.SelectedItemBound = SelectedItemBound;
		}

		#region implemented abstract members of BaseAdapter

		public override Java.Lang.Object GetItem (int position)
		{
			return null;
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override Android.Views.View GetView (int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
		{
			Presentation presentation = presentations[position];
			View view = (convertView ?? context.LayoutInflater.Inflate(Resource.Layout.SlidesItem, parent, false)) as LinearLayout;

			TextView tvName = view.FindViewById<TextView>(Resource.Id.tvName);
			tvName.Text = presentation.Name.Replace("<br />", "\n");

			if (SelectedItemUID.HasValue && presentation.PresentationUID == SelectedItemUID.Value)
			{
				view.SetBackgroundColor(Android.Graphics.Color.Rgb(49, 182, 231));
				this.SelectedItem = view;

				if (SelectedItemBound != null)
					SelectedItemBound();
			}
			else
				view.SetBackgroundColor(Android.Graphics.Color.Black);

			return view;
		}

		public override int Count 
		{
			get 
			{
				if (this.presentations == null)
					return 0;
				else
					return this.presentations.Count;
			}
		}

		#endregion

		public Presentation GetPresentation (int checkedItemPosition)
		{
			if (presentations == null)
				return null;
			else
				return presentations[checkedItemPosition];
		}

		public List<Presentation> GetData
		{
			get
			{
				return this.presentations;
			}
		}

		public void Remove(Guid presentationUID, Guid? selectedItemUID = null)
		{
			if (presentations == null)
				return;

			SelectedItemUID = selectedItemUID;

			presentations.Remove(presentations.Where(p => p.PresentationUID == presentationUID).FirstOrDefault());

			NotifyDataSetChanged();
		}
	}

}

