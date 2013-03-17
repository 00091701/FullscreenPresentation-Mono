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


using Android.App;
using Android.OS;
using Android.Views;
using Android.Graphics;
using Android.Widget;
using Android.Views.Animations;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Libs.FP.Data.Types;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.AndroidHelper;
using System.Threading.Tasks;
using System;
using De.Dhoffmann.Mono.FullscreenPresentation.Buslog;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Droid.Screens
{
	[Activity (Label = "@string/app_name", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape)]
	public class EditActivity : BaseActivity, Animation.IAnimationListener
	{
		enum PresentationSelectionType
		{
			Show,
			Hide
		}

		enum PresentationsMenu : int
		{
			Start = 0,
			Create,
			Rename,
			Delete,
			ShowFolder
		}

		PresentationsHelper presentationsHelper;
		int selectedLongClickItemPosition;
		ListView lvSlides;
		const int animDuration = 500;
		PresentationSelectionType PresentationSelection;
		Animation leftOutAnim;
		Animation rightOutAnim;
		Animation leftInAnim;
		Animation rightInAnim;

		LinearLayout llChoosePresentation;
		LinearLayout llPresentationInfo;


		#region IAnimationListener implementation

		public void OnAnimationEnd (Animation animation)
		{
			if (PresentationSelection == PresentationSelectionType.Hide)
			{
				llChoosePresentation.Visibility = ViewStates.Gone;
				llPresentationInfo.Visibility = ViewStates.Gone;
			}
			else
			{
				llChoosePresentation.Visibility = ViewStates.Visible;
				llPresentationInfo.Visibility = ViewStates.Visible;
			}
		}

		public void OnAnimationRepeat (Animation animation)
		{

		}

		public void OnAnimationStart (Animation animation)
		{

		}

		#endregion

		public EditDetailFragment FragEditDetail {
			get;
			set;
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Logging.Log (this, Logging.LoggingTypeDebug, "OnCreate()");

			SetContentView(Resource.Layout.Edit);

			presentationsHelper = new PresentationsHelper (this);

			Display display = WindowManager.DefaultDisplay;
			Point size = new Point ();
			display.GetSize (size);
			int screenWidth = size.X;
			int screenHeight = size.Y;

			// define animations
			DefineAnimations (screenWidth);

			// set default values
			llChoosePresentation = FindViewById<LinearLayout> (Resource.Id.llChoosePresentation);
			llPresentationInfo = FindViewById<LinearLayout> (Resource.Id.llPresentationInfo);

			llChoosePresentation.SetX(0);
			llChoosePresentation.LayoutParameters = new RelativeLayout.LayoutParams (300, screenHeight);

			llPresentationInfo.SetX (300);
			llPresentationInfo.LayoutParameters = new RelativeLayout.LayoutParams (screenWidth - 300, screenHeight);

			selectedLongClickItemPosition= -1;
			
			lvSlides = FindViewById<ListView>(Resource.Id.lvSlides);
			lvSlides.ItemClick += LvSlides_ItemClick;

			LoadSlidesList ();

			RegisterForContextMenu(lvSlides);
		}

		public void LoadSlidesList ()
		{
			// Präsentationen laden
			presentationsHelper.LoadSlidesListAsync().ContinueWith (t => {
				if (t.IsFaulted)
					return; 
				
				lvSlides.Adapter = new SlidesAdapter(this, t.Result);
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		void DefineAnimations(int screenWidth)
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "DefineAnimations()");

			// define animations
			leftOutAnim = new TranslateAnimation(0, -300, 0, 0);
			leftOutAnim.Duration = animDuration;
			leftOutAnim.SetAnimationListener(this);
			
			rightOutAnim = new TranslateAnimation (0, screenWidth, 0, 0);
			rightOutAnim.Duration = animDuration;
			rightOutAnim.SetAnimationListener (this);
			
			leftInAnim = new TranslateAnimation (-300, 0, 0, 0);
			leftInAnim.Duration = animDuration;
			leftInAnim.SetAnimationListener (this);
			
			rightInAnim = new TranslateAnimation (screenWidth, 0, 0, 0);
			rightInAnim.Duration = animDuration;
			rightInAnim.SetAnimationListener (this);
		}

		void LvSlides_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "LvSlides_ItemClick()");

			// Ausgewählte Zeile markieren.
			e.View.SetBackgroundColor(Android.Graphics.Color.Rgb(49, 182, 231));
			HidePresentationSelection();

			Task.Factory.StartNew (() => {
				SlidesAdapter adapter = lvSlides.Adapter as SlidesAdapter;
				return adapter.GetPresentation (e.Position);
			}).ContinueWith (t => {
				FragEditDetail.LoadPresentation (t.Result);
			}, TaskScheduler.FromCurrentSynchronizationContext ());
		}

		public void ShowPresentationSelection()
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "ShowPresentationSelection()");

			RunOnUiThread (() => {

				PresentationSelection = PresentationSelectionType.Show;

				// Load current list
				presentationsHelper.LoadSlidesListAsync().ContinueWith(t => {
					if (!t.IsFaulted)
					{
						lvSlides.Adapter = new SlidesAdapter(this, t.Result);
					}
				}, TaskScheduler.FromCurrentSynchronizationContext());
			
				llChoosePresentation.Visibility = ViewStates.Visible;
				llPresentationInfo.Visibility = ViewStates.Visible;
			
				// start animation
				llChoosePresentation.StartAnimation (leftInAnim);
				llPresentationInfo.StartAnimation (rightInAnim);
			});
		}

		public void HidePresentationSelection()
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "HidePresentationSelection()");

			RunOnUiThread (() => {
				PresentationSelection = PresentationSelectionType.Hide;

				// start animation
				llChoosePresentation.StartAnimation (leftOutAnim);
				llPresentationInfo.StartAnimation (rightOutAnim);
			});
		}

		public override void OnCreateContextMenu (IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
		{
			base.OnCreateContextMenu (menu, v, menuInfo);
			Logging.Log (this, Logging.LoggingTypeDebug, "OnCreateContextMenu()");

			selectedLongClickItemPosition = ((AdapterView.AdapterContextMenuInfo)menuInfo).Position;
			
			menu.SetHeaderTitle(Resource.String.PresentationsMenuTitle);
			menu.Add(0, (int)PresentationsMenu.Start, 0, Resource.String.PresentationsMenuStart);
			menu.Add(0, (int)PresentationsMenu.Create, 1, Resource.String.PresentationsMenuCreate);
			menu.Add(0, (int)PresentationsMenu.Rename, 2, Resource.String.PresentationsMenuRename);
			menu.Add(0, (int)PresentationsMenu.Delete, 3, Resource.String.PresentationsMenuDelete);
			menu.Add(0, (int)PresentationsMenu.ShowFolder, 4, Resource.String.PresentationsMenuShowFolder);
		}
		
		public override bool OnContextItemSelected (IMenuItem item)
		{
			Logging.Log (this, Logging.LoggingTypeDebug, "OnContextItemSelected()");

			Presentation presentation = ((SlidesAdapter)lvSlides.Adapter).GetPresentation(selectedLongClickItemPosition); 

			if (presentation != null) 
			{
				switch ((PresentationsMenu)item.ItemId) {
				case PresentationsMenu.Start:

					presentationsHelper.StartPresentation (presentation);
					break;
					
				case PresentationsMenu.Create:
					presentationsHelper.CreatePresentation (presentation);
					break;
					
				case PresentationsMenu.Rename:
					presentationsHelper.RenamePresentation (presentation);
					break;
					
				case PresentationsMenu.Delete:
					presentationsHelper.DeletePresentation (presentation);
					break;
					
				case PresentationsMenu.ShowFolder:
					presentationsHelper.ShowPresentationsFolder (presentation);
					break;
				}
			}
			
			selectedLongClickItemPosition = -1;
			return base.OnContextItemSelected (item);
		}
	}
}
