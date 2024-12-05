//using Android.OS;
using Microsoft.Maui.Layouts;

namespace SpiritIsland.Maui;

static public class AbsoluteLayout_Extensions {

	static public void Float( this AbsoluteLayout absoluteLayout, IView view, Rect bounds ) {
		ArgumentNullException.ThrowIfNull( view );

		absoluteLayout.Add( view );
		absoluteLayout.SetLayoutBounds( view, bounds );
		absoluteLayout.SetLayoutFlags( view, AbsoluteLayoutFlags.None);
	}

	static public void UpdateFloat( this AbsoluteLayout absoluteLayout, IView view, Rect newBounds) {
		absoluteLayout.SetLayoutBounds(view, newBounds);
	}

}