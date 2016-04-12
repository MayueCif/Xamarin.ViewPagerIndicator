using System;
using Android.Views;
using Android.Content;
using Android.Util;
using Android.Graphics;
using Android.Support.V4.View;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Java.Lang;
using Android.OS;
using Java.Interop;

namespace Xamarin.ViewPagerIndicator
{
	public class LinePageIndicator:View,IPageIndicator
	{

		private const int INVALID_POINTER = -1;

		private readonly Paint paintUnselected = new Paint (PaintFlags.AntiAlias);
		private readonly Paint paintSelected = new Paint (PaintFlags.AntiAlias);
		private ViewPager viewPager;
		private ViewPager.IOnPageChangeListener listener;
		private int currentPage;

		private bool centered;

		public bool Centered {
			get{ return centered; }
			set { 
				centered = value;
				Invalidate ();
			}
		}

		private float lineWidth;

		public float LineWidth {
			get{ return lineWidth; }
			set { 
				lineWidth = value;
				Invalidate ();
			}
		}

		private float gapWidth;

		public float GapWidth {
			get{ return gapWidth; }
			set { 
				gapWidth = value; 
				Invalidate ();
			}
		}

		private int mTouchSlop;
		private float mLastMotionX = -1;
		private int mActivePointerId = INVALID_POINTER;
		private bool mIsDragging;

		public LinePageIndicator (Context context) : this (context, null)
		{
			
		}

		public LinePageIndicator (Context context, IAttributeSet attrs) : this (context, attrs, Resource.Attribute.vpiLinePageIndicatorStyle)
		{
			
		}

		public LinePageIndicator (Context context, IAttributeSet attrs, int defStyle) : base (context, attrs, defStyle)
		{
			if (IsInEditMode)
				return;

			//Load defaults from resources
			int defaultSelectedColor = Resources.GetColor (Resource.Color.default_line_indicator_selected_color);
			int defaultUnselectedColor = Resources.GetColor (Resource.Color.default_line_indicator_unselected_color);
			float defaultLineWidth = Resources.GetDimension (Resource.Dimension.default_line_indicator_line_width);
			float defaultGapWidth = Resources.GetDimension (Resource.Dimension.default_line_indicator_gap_width);
			float defaultStrokeWidth = Resources.GetDimension (Resource.Dimension.default_line_indicator_stroke_width);
			bool defaultCentered = Resources.GetBoolean (Resource.Boolean.default_line_indicator_centered);

			//Retrieve styles attributes
			TypedArray a = context.ObtainStyledAttributes (attrs, Resource.Styleable.LinePageIndicator, defStyle, 0);

			centered = a.GetBoolean (Resource.Styleable.LinePageIndicator_centered, defaultCentered);
			lineWidth = a.GetDimension (Resource.Styleable.LinePageIndicator_lineWidth, defaultLineWidth);
			gapWidth = a.GetDimension (Resource.Styleable.LinePageIndicator_gapWidth, defaultGapWidth);
			SetStrokeWidth (a.GetDimension (Resource.Styleable.LinePageIndicator_strokeWidth, defaultStrokeWidth));
			paintUnselected.Color = a.GetColor (Resource.Styleable.LinePageIndicator_unselectedColor, defaultUnselectedColor);
			paintSelected.Color = a.GetColor (Resource.Styleable.LinePageIndicator_selectedColor, defaultSelectedColor);

			Drawable background = a.GetDrawable (Resource.Styleable.LinePageIndicator_android_background);
			if (background != null) {
				SetBackgroundDrawable (background);
			}

			a.Recycle ();

			ViewConfiguration configuration = ViewConfiguration.Get (context);
			mTouchSlop = ViewConfigurationCompat.GetScaledPagingTouchSlop (configuration);
		}


		public void SetUnselectedColor (Color unselectedColor)
		{
			paintUnselected.Color = unselectedColor;
			Invalidate ();
		}

		public void SetSelectedColor (Color selectedColor)
		{
			paintSelected.Color = selectedColor;
			Invalidate ();
		}


		public void SetStrokeWidth (float lineHeight)
		{
			paintSelected.StrokeWidth = lineHeight;
			paintUnselected.StrokeWidth = lineHeight;
			Invalidate ();
		}


		protected override void OnDraw (Canvas canvas)
		{
			base.OnDraw (canvas);

			if (viewPager == null) {
				return;
			}
			int count = viewPager.Adapter.Count;
			if (count == 0) {
				return;
			}

			if (currentPage >= count) {
				SetCurrentItem (count - 1);
				return;
			}

			float lineWidthAndGap = lineWidth + gapWidth;
			float indicatorWidth = (count * lineWidthAndGap) - gapWidth;
			float paddingTop = PaddingTop;
			float paddingLeft = PaddingLeft;
			float paddingRight = PaddingRight;

			float verticalOffset = paddingTop + ((Height - paddingTop - PaddingBottom) / 2.0f);
			float horizontalOffset = paddingLeft;
			if (centered) {
				horizontalOffset += ((Width - paddingLeft - paddingRight) / 2.0f) - (indicatorWidth / 2.0f);
			}

			//Draw stroked circles
			for (int i = 0; i < count; i++) {
				float dx1 = horizontalOffset + (i * lineWidthAndGap);
				float dx2 = dx1 + lineWidth;
				canvas.DrawLine (dx1, verticalOffset, dx2, verticalOffset, (i == currentPage) ? paintSelected : paintUnselected);
			}
		}


		public override bool OnTouchEvent (MotionEvent e)
		{
			if (base.OnTouchEvent (e)) {
				return true;
			}
			if ((viewPager == null) || (viewPager.Adapter.Count == 0)) {
				return false;
			}

			var action = (int)e.Action & MotionEventCompat.ActionMask;
			switch (action) {
			case (int)MotionEventActions.Down:
				mActivePointerId = MotionEventCompat.GetPointerId (e, 0);
				mLastMotionX = e.GetX ();
				break;

			case (int)MotionEventActions.Move:
				{
					int activePointerIndex = MotionEventCompat.FindPointerIndex (e, mActivePointerId);
					float x = MotionEventCompat.GetX (e, activePointerIndex);
					float deltaX = x - mLastMotionX;

					if (!mIsDragging) {
						if (Java.Lang.Math.Abs (deltaX) > mTouchSlop) {
							mIsDragging = true;
						}
					}

					if (mIsDragging) {
						mLastMotionX = x;
						if (viewPager.IsFakeDragging || viewPager.BeginFakeDrag ()) {
							viewPager.FakeDragBy (deltaX);
						}
					}

					break;
				}

			case (int)MotionEventActions.Cancel:
			case (int)MotionEventActions.Up:
				if (!mIsDragging) {
					int count = viewPager.Adapter.Count;
					int width = Width;
					float halfWidth = width / 2f;
					float sixthWidth = width / 6f;

					if ((currentPage > 0) && (e.GetX () < halfWidth - sixthWidth)) {
						if (action != (int)MotionEventActions.Cancel) {
							viewPager.CurrentItem = currentPage - 1;
						}
						return true;
					} else if ((currentPage < count - 1) && (e.GetX () > halfWidth + sixthWidth)) {
						if (action != (int)MotionEventActions.Cancel) {
							viewPager.CurrentItem = currentPage + 1;
						}
						return true;
					}
				}

				mIsDragging = false;
				mActivePointerId = INVALID_POINTER;
				if (viewPager.IsFakeDragging)
					viewPager.EndFakeDrag ();
				break;

			case (int)MotionEventCompat.ActionPointerDown:
				{
					int index = MotionEventCompat.GetActionIndex (e);
					mLastMotionX = MotionEventCompat.GetX (e, index);
					mActivePointerId = MotionEventCompat.GetPointerId (e, index);
					break;
				}

			case (int)MotionEventCompat.ActionPointerUp:
				int pointerIndex = MotionEventCompat.GetActionIndex (e);
				int pointerId = MotionEventCompat.GetPointerId (e, pointerIndex);
				if (pointerId == mActivePointerId) {
					int newPointerIndex = pointerIndex == 0 ? 1 : 0;
					mActivePointerId = MotionEventCompat.GetPointerId (e, newPointerIndex);
				}
				mLastMotionX = MotionEventCompat.GetX (e, MotionEventCompat.FindPointerIndex (e, mActivePointerId));
				break;
			}

			return true;
		}

		#region 实现接口IPageIndicator

		public void SetViewPager (ViewPager viewPager)
		{
			if (this.viewPager == viewPager) {
				return;
			}
			if (this.viewPager != null) {
				//Clear us from the old pager.
				//this.viewPager.setOnPageChangeListener(null);
				this.viewPager.ClearOnPageChangeListeners ();
			}
			if (viewPager.Adapter == null) {
				throw new IllegalStateException ("ViewPager does not have adapter instance.");
			}
			this.viewPager = viewPager;
			this.viewPager.AddOnPageChangeListener (this);
			Invalidate ();
		}

		public void SetViewPager (ViewPager view, int initialPosition)
		{
			SetViewPager (view);
			SetCurrentItem (initialPosition);
		}


		public void SetCurrentItem (int item)
		{
			if (viewPager == null) {
				throw new IllegalStateException ("ViewPager has not been bound.");
			}
			viewPager.CurrentItem = item;
			currentPage = item;
			Invalidate ();
		}


		public void NotifyDataSetChanged ()
		{
			Invalidate ();
		}

		public void SetOnPageChangeListener (ViewPager.IOnPageChangeListener listener)
		{
			this.listener = listener;
		}

		#endregion


		#region 实现接口 ViewPager.IOnPageChangeListener


		public void OnPageScrolled (int position, float positionOffset, int positionOffsetPixels)
		{
			if (listener != null) {
				listener.OnPageScrolled (position, positionOffset, positionOffsetPixels);
			}
		}

		public void OnPageSelected (int position)
		{
			currentPage = position;
			Invalidate ();

			if (listener != null) {
				listener.OnPageSelected (position);
			}
		}


		public void OnPageScrollStateChanged (int state)
		{
			if (listener != null) {
				listener.OnPageScrollStateChanged (state);
			}
		}

		#endregion



		protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
		{
			SetMeasuredDimension (MeasureWidth (widthMeasureSpec), MeasureHeight (heightMeasureSpec));
		}


		/// <summary>
		/// Determines the width of this view
		/// </summary>
		/// <returns>The width.</returns>
		/// <param name="measureSpec">Measure spec.</param>
		private int MeasureWidth (int measureSpec)
		{
			float result;
			var specMode = MeasureSpec.GetMode (measureSpec);
			var specSize = MeasureSpec.GetSize (measureSpec);

			if ((specMode == MeasureSpecMode.Exactly) || (viewPager == null)) {
				//We were told how big to be
				result = specSize;
			} else {
				//Calculate the width according the views count
				int count = viewPager.Adapter.Count;
				result = PaddingLeft + PaddingRight + (count * lineWidth) + ((count - 1) * gapWidth);
				//Respect AT_MOST value if that was what is called for by measureSpec
				if (specMode == MeasureSpecMode.AtMost) {
					result = Java.Lang.Math.Min (result, specSize);
				}
			}
			return (int)FloatMath.Ceil (result);
		}

		/// <summary>
		/// Determines the height of this view
		/// </summary>
		/// <returns>The height.</returns>
		/// <param name="measureSpec">Measure spec.</param>
		private int MeasureHeight (int measureSpec)
		{
			float result;
			var specMode = MeasureSpec.GetMode (measureSpec);
			var specSize = MeasureSpec.GetSize (measureSpec);

			if (specMode == MeasureSpecMode.Exactly) {
				//We were told how big to be
				result = specSize;
			} else {
				//Measure the height
				result = paintSelected.StrokeWidth + PaddingTop + PaddingBottom;
				//Respect AT_MOST value if that was what is called for by measureSpec
				if (specMode == MeasureSpecMode.AtMost) {
					result = Java.Lang.Math.Min (result, specSize);
				}
			}
			return (int)FloatMath.Ceil (result);
		}


		protected override void OnRestoreInstanceState (Android.OS.IParcelable state)
		{
			base.OnRestoreInstanceState (state);
			SavedState savedState = (SavedState)state;
			currentPage = savedState.CurrentPage;
			RequestLayout ();
		}


		protected override Android.OS.IParcelable OnSaveInstanceState ()
		{

			var superState = base.OnSaveInstanceState ();
			var savedState = new SavedState (superState) {
				CurrentPage = currentPage
			};
			return savedState;
		}

		public class SavedState:Android.Views.View.BaseSavedState
		{
			public int CurrentPage { get; set; }

			public SavedState (IParcelable superState) : base (superState)
			{

			}

			private SavedState (Parcel source) : base (source)
			{
				CurrentPage = source.ReadInt ();
			}

			public override void WriteToParcel (Parcel dest, ParcelableWriteFlags flags)
			{
				base.WriteToParcel (dest, flags);
				dest.WriteInt (CurrentPage);
			}

			[ExportField ("CREATOR")]
			public static SavedStateCreator InitializeCreator ()
			{
				return new SavedStateCreator ();
			}

			public class SavedStateCreator : Java.Lang.Object, IParcelableCreator
			{
				public Java.Lang.Object CreateFromParcel (Parcel source)
				{
					return new SavedState (source);
				}

				public Java.Lang.Object[] NewArray (int size)
				{
					return new SavedState[size];
				}
			}
		}


	}


}

