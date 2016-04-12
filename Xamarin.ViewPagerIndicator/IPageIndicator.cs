using System;
using Android.Views;
using Android.Support.V4.View;

namespace Xamarin.ViewPagerIndicator
{
	/// <summary>
	/// A PageIndicator is responsible to show an visual indicator on the total views number and the current visible view.
	/// </summary>
	public interface IPageIndicator:ViewPager.IOnPageChangeListener
	{
		/// <summary>
		/// Bind the indicator to a ViewPager.
		/// </summary>
		/// <param name="view">View.</param>
		void SetViewPager (ViewPager view);

		void SetViewPager (ViewPager view, int initialPosition);

		/// <summary>
		/// Set the current page of both the ViewPager and indicator.
		/// </summary>
		/// <param name="item">Item.</param>
		void SetCurrentItem (int item);

		/// <summary>
		/// Set a page change listener which will receive forwarded events.
		/// </summary>
		/// <param name="listener">Listener.</param>
		void SetOnPageChangeListener (ViewPager.IOnPageChangeListener listener);

		/// <summary>
		/// Notify the indicator that the fragment list has changed.
		/// </summary>
		void NotifyDataSetChanged ();

	}

}

