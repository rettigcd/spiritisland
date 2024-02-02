namespace SpiritIsland;

public abstract class BasePaintableRect : IPaintableRect {

	public virtual float? WidthRatio { get; set; }
	public BrushSpec? Background {get;set;}
	public PenSpec? Border {get;set;}

	/// <summary> (% of Min(width,height)) to add around the border</summary>
	public PaddingSpec Padding = PaddingSpec.None;
	public PaddingSpec Margin = PaddingSpec.None;

	public virtual void Paint( Graphics graphics, Rectangle bounds ){
		bounds = Margin.Content(bounds);
		Background?.Fill(graphics,bounds);
		PaintContent(graphics,Padding.Content(bounds));
		Border?.Stroke(graphics,bounds); // doing last to clean-up edges
	}
	abstract protected void PaintContent(Graphics graphics,Rectangle bounds);
}
