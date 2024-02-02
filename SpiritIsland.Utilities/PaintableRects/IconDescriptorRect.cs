namespace SpiritIsland;

public class IconDescriptorRect( IconDescriptor descriptor ) : IPaintableRect {

	public float? WidthRatio { get; set; }

	public void Paint( Graphics graphics, Rectangle rect ) {
		// !!! Instead of drawing this directly, use TrackSlot to cache the image.
		IconDrawer.DrawTheIcon( graphics, _descriptor, rect, _resources );
	}

	readonly static IconResources _resources = ResourceImages.Singleton;

	readonly IconDescriptor _descriptor = descriptor;

}
