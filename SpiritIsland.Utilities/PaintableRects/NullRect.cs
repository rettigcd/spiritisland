namespace SpiritIsland;

public class NullRect : IPaintableRect {

	public float? WidthRatio { get; set; }

	public void Paint( Graphics graphics, Rectangle bounds ){ }
}
