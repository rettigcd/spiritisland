using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpiritIsland.WinForms;

static public class GraphicsExtensions {

	#region Draw Images / Super-script - Subscript text

	readonly static Font superSubScriptFont = new( "Arial", 7, FontStyle.Bold, GraphicsUnit.Point );

	static public void DrawCountIfHigherThan( this Graphics graphics, RectangleF rect, int count, int highestHidden = 1 ) {
		if(count > highestHidden)
			DrawSubscript(graphics, rect.ToInts(), "x" + count);
	}

	static public void DrawSubscript(this Graphics graphics, Rectangle rect, string txt ) {
		SizeF sz = graphics.MeasureString( txt, superSubScriptFont );
		var numRect = new RectangleF( rect.Right - sz.Width+2, rect.Bottom - sz.Height, sz.Width + 3, sz.Height + 2 );
		graphics.FillEllipse( Brushes.White, numRect );
		graphics.DrawEllipse( Pens.Black, numRect );
		graphics.DrawString( txt, superSubScriptFont, Brushes.Black, numRect.X + 2, numRect.Y + 2 );
	}

	static public void DrawSuperscript( this Graphics graphics, Rectangle rect, string txt ) {
		SizeF sz = graphics.MeasureString( txt, superSubScriptFont );
		var numRect = new RectangleF( rect.Right - sz.Width + 2, rect.Top - sz.Height, sz.Width + 3, sz.Height + 2 );
		graphics.FillEllipse( Brushes.White, numRect );
		graphics.DrawEllipse( Pens.Black, numRect );
		graphics.DrawString( txt, superSubScriptFont, Brushes.Black, numRect.X + 2, numRect.Y + 2 );
	}

	static public void DrawImageFitHeight( this Graphics graphics, Image image, Rectangle bounds ) {
		graphics.DrawImage( image, bounds.FitHeight( image.Size ) );
	}

	static public void DrawImageFitWidth( this Graphics graphics, Image image, RectangleF bounds ) {
		graphics.DrawImage( image, bounds.ToInts().FitWidth( image.Size ) );
	}

	static public void DrawImageFitBoth( this Graphics graphics, Image image, RectangleF bounds ) {
		graphics.DrawImage( image, bounds.ToInts().FitBoth( image.Size ) );
	}

	static public void DrawInvaderCardFront( this Graphics graphics, RectangleF rect, InvaderCard card ) {
		if(card == null) return;
		using Image img = ResourceImages.Singleton.GetInvaderCard( card );
		graphics.DrawImage( img, rect );
	}

	static public void DrawStringCenter( this Graphics graphics, string text, Font labelFont, Brush brush, RectangleF textBounds ) {
		using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };
		graphics.DrawString( text, labelFont, brush, textBounds, alignCenter );
	}

	#endregion

	#region Rounded-Corner Rectangles

	public static GraphicsPath RoundCorners( this Rectangle bounds, int radius, bool tl=true, bool tr=true, bool br=true, bool bl=true ) {
		int diameter = radius * 2;
		Size arcSize = new Size( diameter, diameter );
		Rectangle arc = new Rectangle( bounds.Location, arcSize );
		GraphicsPath path = new GraphicsPath();

		if(radius <= 0)
			tl=tr=br=bl=false;

		// top left arc  
		if(tl)
			path.AddArc( arc, 180, 90 );
		else
			path.AddLine( arc.Location, new Point( arc.Right, arc.Y ) );

		// top right arc  
		arc.X = bounds.Right - diameter;
		if(tr)
			path.AddArc( arc, 270, 90 );
		else
			path.AddLine( new Point( arc.Right, arc.Y ), new Point( arc.Right, arc.Bottom ) );

		// bottom right arc  
		arc.Y = bounds.Bottom - diameter;
		if(br)
			path.AddArc( arc, 0, 90 );
		else
			path.AddLine( new Point( arc.Right, arc.Bottom ), new Point( arc.X, arc.Bottom ) );

		// bottom left arc 
		arc.X = bounds.Left;
		if(bl)
			path.AddArc( arc, 90, 90 );
		else
			path.AddLine( new Point(arc.X,arc.Bottom), arc.Location);

		path.CloseFigure();
		return path;
	}

	public static void DrawRoundedRectangle( this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius ) {
		using GraphicsPath path = bounds.RoundCorners( cornerRadius );
		graphics.DrawPath( pen, path );
	}

	public static void FillRoundedRectangle( this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius ) {
		using GraphicsPath path = bounds.RoundCorners( cornerRadius );
		graphics.FillPath( brush, path );
	}

	#endregion Rounded-Corner Rectangles

	public static void Deconstruct<T>( this T[] r, out T r0, out SubArray<T> rest ) { (r0,rest) = new SubArray<T>( r );	}

}

public enum Align { 
	Default, 
	/// <summary> Left / Top </summary>
	Near, 
	Center,
	/// <summary> Right / Bottom </summary>
	Far
}

public readonly struct SubArray<T> {
	readonly int _start;
	readonly T[] _array;
	public SubArray( T[] array, int start=0 ) {  _array = array; _start = start; }
	public void Deconstruct( out T t, out SubArray<T> rest ) {
		if( _start == _array.Length ) throw new ArgumentOutOfRangeException($"Cannot extract element[{_start}] of .Length={_start} array.");
		t = _array[_start];
		rest = new SubArray<T>(_array,_start+1);
	}
}