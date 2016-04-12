using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Xamarin.ViewPagerIndicator;

namespace Xamarin.ViewPagerIndicatorTest
{
	[Activity (Label = "Xamarin.ViewPagerIndicatorTest", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : FragmentActivity
	{

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			ViewPager pager = FindViewById<ViewPager> (Resource.Id.pager);
			pager.Adapter = new SectionsPagerAdapter (SupportFragmentManager);

			//Bind the title indicator to the adapter
			LinePageIndicator lineIndicator = FindViewById<LinePageIndicator> (Resource.Id.indicator);
			lineIndicator.SetViewPager (pager);
		}
	}


	public class PlaceholderFragment:Android.Support.V4.App.Fragment
	{

		private const string ARG_SECTION_NUMBER = "section_number";


		/// <summary>
		/// Returns a new instance of this fragment for the given section number.
		/// </summary>
		/// <returns>The instance.</returns>
		/// <param name="sectionNumber">Section number.</param>
		public static PlaceholderFragment NewInstance (int sectionNumber)
		{
			PlaceholderFragment fragment = new PlaceholderFragment ();
			Bundle args = new Bundle ();
			args.PutInt (ARG_SECTION_NUMBER, sectionNumber);
			fragment.Arguments = args;
			return fragment;
		}

		public override Android.Views.View OnCreateView (Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
		{
			View rootView = inflater.Inflate (Resource.Layout.fragment_main, container, false);
			TextView textView = rootView.FindViewById<TextView> (Resource.Id.section_label);
			textView.Text = GetString (Resource.String.section_format, Arguments.GetInt (ARG_SECTION_NUMBER));
			return rootView;

		}

	}



	public class SectionsPagerAdapter:FragmentPagerAdapter
	{

		public SectionsPagerAdapter (Android.Support.V4.App.FragmentManager fm) : base (fm)
		{

		}

		public override Android.Support.V4.App.Fragment GetItem (int position)
		{
			// getItem is called to instantiate the fragment for the given page.
			// Return a PlaceholderFragment (defined as a static inner class below).
			return PlaceholderFragment.NewInstance (position + 1);
		}


		public override int Count {
			get { return 3; }
		}

		public override Java.Lang.ICharSequence GetPageTitleFormatted (int position)
		{
			switch (position) {
			case 0:
				return new Java.Lang.String ("SECTION 1");
			case 1:
				return new Java.Lang.String ("SECTION 2");
			case 2:
				return new Java.Lang.String ("SECTION 3");
			}
			return null;
		}

	}
}


