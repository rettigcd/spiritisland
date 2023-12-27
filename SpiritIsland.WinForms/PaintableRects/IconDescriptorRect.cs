using System.Drawing;

namespace SpiritIsland.WinForms;

class IconDescriptorRect : IPaintableRect {

	static IconResources _resources = ResourceImages.Singleton;

	readonly IconDescriptor _descriptor;
	public IconDescriptorRect( IconDescriptor descriptor ) {
		_descriptor = descriptor;
	}
	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		// !!! Instead of drawing this directly, use TrackSlot to cache the image.
		IconDrawer.DrawTheIcon( graphics, _descriptor, rect, _resources );
		return rect;
	}
}
