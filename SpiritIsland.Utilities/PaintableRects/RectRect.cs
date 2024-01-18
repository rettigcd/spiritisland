using System.Drawing;

namespace SpiritIsland;

public class RectRect : IPaintableRect {

	public RectRect() {}
	/// <summary> Hex Color </summary>
	public string? Fill { get; set; }
	/// <summary> Hex Color </summary>
	public string? Stroke { get; set; }

	public Rectangle Paint( Graphics graphics, Rectangle bounds ) {

		// 				graphics.FillPath( outerBrush, bounds.RoundCorners( 20 ) );
		if(_cornerRadius == 0f)
			PaintSquareCorners( graphics, bounds );
		else
			PaintRoundedCorners( graphics, bounds );
		return bounds;
	}

	void PaintRoundedCorners( Graphics graphics, Rectangle bounds ) {
		int radius = (int)(Math.Min(bounds.Width,bounds.Height) * _cornerRadius);
		var path = bounds.RoundCorners( radius, _tl, _tr, _br, _bl );
		if(Fill is not null)
			using(var brush = new SolidBrush( ColorString.ParseHexColor( Fill ) ))
				graphics.FillPath( brush, path );
		if(Stroke is not null)
			using(var pen = new Pen( ColorString.ParseHexColor( Stroke ) ))
				graphics.DrawPath( pen, path );
	}

	void PaintSquareCorners( Graphics graphics, Rectangle bounds ) {
		if(Fill is not null)
			using(var brush = new SolidBrush( ColorString.ParseHexColor( Fill ) ))
				graphics.FillRectangle( brush, bounds );
		if(Stroke is not null)
			using(var pen = new Pen( ColorString.ParseHexColor( Stroke ) ))
				graphics.DrawRectangle( pen, bounds );
	}

	bool _tl,_tr,_bl,_br;
	float _cornerRadius = 0f; // % of min dimension
	public RectRect RoundCorners( float radias ) { _cornerRadius = radias; _tl=_tr=_bl=_br=true; return this; }
	public RectRect RoundCorners( float radias, bool tl, bool tr, bool br, bool bl ) { _cornerRadius = radias; _tl=tl; _tr=tr; _bl=bl;_br=br; return this; }
}
