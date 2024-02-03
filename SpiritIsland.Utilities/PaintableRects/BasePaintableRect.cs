namespace SpiritIsland;

public abstract class BasePaintableRect : IPaintableRect {

	public virtual float? WidthRatio { get; set; }
	public BrushSpec? Background {get;set;}
	public PenSpec? Border {get;set;}

	/// <summary> returns the last Bounds less the Margin. </summary>
	public Rectangle Bounds {get; private set;}

	/// <summary> (% of Min(width,height)) to add around the border</summary>
	public PaddingSpec Padding = PaddingSpec.None;
	public PaddingSpec Margin = PaddingSpec.None;

	public virtual void Paint( Graphics graphics, Rectangle bounds ){
		Bounds = Margin.Content(bounds);
		Background?.Fill(graphics,Bounds);
		PaintContent(graphics,Padding.Content(Bounds));
		Border?.Stroke(graphics,Bounds); // doing last to clean-up edges
	}
	abstract protected void PaintContent(Graphics graphics,Rectangle bounds);
}
