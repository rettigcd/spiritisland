using System;
using System.Drawing;

namespace SpiritIsland.WinForms; 

class InnatePainter( InnatePower power, InnateLayout layout ) : IDisposable {

	#region constructor

	#endregion

	public void DrawFromLayout( Graphics graphics,  ImgMemoryCache _ ) {

		_backgroundCache ??= DrawBackgroundImage();

		// -- Background Layer --
		graphics.DrawImage( _backgroundCache, _layout.Bounds );

	}

	Bitmap DrawBackgroundImage() {
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

		DrawAttributeTable( graphics );
		return backgroundCache;
	}

	#region private

	void DrawAttributeTable( Graphics graphics ) {
		// Attribute Headers
		using(var titleBg = new SolidBrush( Color.FromArgb( 0xae, 0x98, 0x69 ) )) // ae9869
			graphics.FillRectangle( titleBg, _layout.AttributeRows[0] );
		using var centerBoth = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
		using(Font titleFont = new Font( "Arial", _layout._textEmSize * 0.8f, FontStyle.Bold, GraphicsUnit.Pixel )) {
			graphics.DrawString( "SPEED", titleFont, Brushes.White, _layout.AttributeLabelCells[0], centerBoth );
			graphics.DrawString( "RANGE", titleFont, Brushes.White, _layout.AttributeLabelCells[1], centerBoth );
			graphics.DrawString( _power.LandOrSpirit == LandOrSpirit.Land ? "TARGET LAND" : "TARGET", titleFont, Brushes.White, _layout.AttributeLabelCells[2], centerBoth );
		}

		// Attribute Values
		PowerHeaderDrawer.DrawAttributeValues( graphics, _layout.AttributeValueCells, _power );

		// Attribute Outter box
		using Pen thickPen = new Pen( Brushes.Black, 2f );
		graphics.DrawRectangle( thickPen, _layout.AttributeBounds );
	}

	public void Dispose() {
		if(_backgroundCache != null) {
			_backgroundCache.Dispose();
			_backgroundCache = null;
		}
	}

	readonly InnatePower _power = power;
	readonly InnateLayout _layout = layout;

	Bitmap _backgroundCache;
	readonly Brush backgroundBrush = Brushes.AliceBlue;

	#endregion

}

