using System;
using System.Drawing;

namespace SpiritIsland.WinForms; 

class InnatePainter : IDisposable {

	#region constructor

	public InnatePainter( InnatePower power, InnateLayout layout ) {
		this._power = power;
		this._layout = layout;
	}

	#endregion

	public void DrawFromLayout( Graphics graphics,  CachedImageDrawer imageDrawer ) {

		if(_backgroundCache == null) {
			this._imageDrawer = imageDrawer;
			using var boldFont = _layout.UsingBoldFont();
			_backgroundCache = DrawBackgroundImage( boldFont );
		}

		// -- Background Layer --
		graphics.DrawImage( _backgroundCache, _layout.Bounds );

	}

	Bitmap DrawBackgroundImage( Font boldFont ) {
		var bounds = _layout.Bounds;
		var backgroundCache = new Bitmap( bounds.Width, bounds.Height );
		using var graphics = Graphics.FromImage( backgroundCache );
		graphics.TranslateTransform(-bounds.X,-bounds.Y);

		graphics.FillRectangle( backgroundBrush, _layout.Bounds );

		// Title
		using(var titleFont = new Font( "Arial", _layout._textEmSize, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel ))
			graphics.DrawString( _power.Name.ToUpper(), titleFont, Brushes.Black, _layout.TitleBounds, new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center } );

		// This could be on the bottom layer
		_layout.GeneralInstructions?.Paint( graphics );

		DrawAttributeTable( graphics, boldFont );
		return backgroundCache;
	}

	#region private

	void DrawAttributeTable( Graphics graphics, Font boldFont ) {
		// Attribute Headers
		using(var titleBg = new SolidBrush( Color.FromArgb( 0xae, 0x98, 0x69 ) )) // ae9869
			graphics.FillRectangle( titleBg, _layout.AttributeRows[0] );
		var centerBoth = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
		using(Font titleFont = new Font( "Arial", _layout._textEmSize * 0.8f, FontStyle.Bold, GraphicsUnit.Pixel )) {
			graphics.DrawString( "SPEED", titleFont, Brushes.White, _layout.AttributeLabelCells[0], centerBoth );
			graphics.DrawString( "RANGE", titleFont, Brushes.White, _layout.AttributeLabelCells[1], centerBoth );
			graphics.DrawString( _power.LandOrSpirit == LandOrSpirit.Land ? "TARGET LAND" : "TARGET", titleFont, Brushes.White, _layout.AttributeLabelCells[2], centerBoth );
		}

		// Attribute Values
		graphics.FillRectangle( Brushes.BlanchedAlmond, _layout.AttributeRows[1] );
		foreach(var valueRect in _layout.AttributeValueCells)
			graphics.DrawRectangle( Pens.Black, valueRect );

		_imageDrawer.DrawFitHeight(
			graphics,
			_power.DisplaySpeed == Phase.Slow ? Img.Icon_Slow : Img.Icon_Fast,
			_layout.AttributeValueCells[0].InflateBy( (int)(-_layout.AttributeValueCells[0].Height * .2f) )
		);

		graphics.DrawString( _power.RangeText, boldFont, Brushes.Black, _layout.AttributeValueCells[1], centerBoth );
		graphics.DrawString( _power.TargetFilter.ToUpper(), boldFont, Brushes.Black, _layout.AttributeValueCells[2], centerBoth );

		// Attribute Outter box
		using var thickPen = new Pen( Brushes.Black, 2f );
		graphics.DrawRectangle( thickPen, _layout.AttributeBounds );
	}

	public void Dispose() {
		if(_backgroundCache != null) {
			_backgroundCache.Dispose();
			_backgroundCache = null;
		}
	}

	readonly InnatePower _power;
	readonly InnateLayout _layout;

	// single threaded variables...
	CachedImageDrawer _imageDrawer;

	Bitmap _backgroundCache;
	readonly Brush backgroundBrush = Brushes.AliceBlue;

	#endregion

}

