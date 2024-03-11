//using Android.OS;
using Microsoft.Maui.Layouts;

namespace SpiritIsland.Maui;

static public class AbsoluteLayout_Extensions {

	static public void Float( this AbsoluteLayout absoluteLayout, IView view, Rect bounds, AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None ) {
		ArgumentNullException.ThrowIfNull( view );

//		if(bounds.IsEmpty)
//			throw new ArgumentNullException( nameof( bounds ) );

		absoluteLayout.Add( view );
		absoluteLayout.SetLayoutBounds( view, bounds );
		absoluteLayout.SetLayoutFlags( view, flags );
	}

}