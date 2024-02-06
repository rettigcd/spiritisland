using System.Drawing.Drawing2D;

namespace SpiritIsland;

/// <summary>
/// Draws / Fills a Rectangle
/// </summary>
public class RectRect : IPaintableRect {

	#region Style

	/// <summary> Hex Color </summary>
	public BrushSpec? Fill {get; set;}

	/// <summary> Hex Color </summary>
	public PenSpec? Stroke { get; set; }

	#endregion Style

	public float? WidthRatio { get; set; }

	public RectRect RoundCorners( float radias ) { _cornerRadius = radias; _tl=_tr=_bl=_br=true; return this; }
	public RectRect RoundCorners( float radias, bool tl, bool tr, bool br, bool bl ) { _cornerRadius = radias; _tl=tl; _tr=tr; _bl=bl;_br=br; return this; }

	#region Paint

	public void Paint( Graphics graphics, Rectangle bounds ) {

		if(_cornerRadius == 0f)
			PaintSquareCorners( graphics, bounds );
		else
			PaintRoundedCorners( graphics, bounds );
	}

	void PaintRoundedCorners( Graphics graphics, Rectangle bounds ) {
		GraphicsPath path = CalcPath(bounds);
		Fill?.Fill(graphics,path);
		Stroke?.DrawPath(graphics,path,bounds);
	}

	void PaintSquareCorners( Graphics graphics, Rectangle bounds ) {
		Fill?.Fill(graphics,bounds);
		Stroke?.DrawRectangle(graphics,bounds);
	}

	#endregion Paint

	#region private

	GraphicsPath CalcPath(Rectangle bounds) => bounds.RoundCorners( CalcRadiusInPixels(bounds), _tl, _tr, _br, _bl );
	int CalcRadiusInPixels(Rectangle bounds) => (int)(Math.Min(bounds.Width,bounds.Height) * _cornerRadius);

	bool _tl,_tr,_bl,_br; // round corner?
	float _cornerRadius = 0f; // % of min dimension
	#endregion private
}
