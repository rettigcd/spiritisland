using System;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	class InnatePainter : IDisposable {

		#region constructor

		public InnatePainter( InnatePower power, InnateLayout layout ) {
			this._power = power;
			this._layout = layout;
		}

		#endregion

		public void DrawFromLayout(  Graphics graphics,  CachedImageDrawer imageDrawer, 
			ElementCounts activatedElements,
			InnatePower[] innateOptions, 
			IDrawableInnateOption[] innateGroupOptions 
		) {

			if(backgroundCache == null) {
				this.imageDrawer = imageDrawer;
				using var boldFont = _layout.UsingBoldFont();
				DrawBackgroundImage( boldFont );
				InitOverlayCache( boldFont );
			}

			// -- Background Layer --
			graphics.DrawImage( backgroundCache, _layout.Bounds );

			this.graphics = graphics;
			// -- Middle Layer - Available --
			foreach(WrappingText_InnateOptions wrappintText in _layout.Options)
				if(wrappintText.InnateOption.IsActive( activatedElements ))
					graphics.FillRectangle( Brushes.PeachPuff, wrappintText.Bounds );

			// Overlay text / images
			graphics.DrawImage( overlayCache, _layout.Bounds );

			// -- Top Layer - Selectable Options --
			DrawTopLayer_SelectableOptions( graphics, innateOptions, innateGroupOptions );

		}

		private void DrawTopLayer_SelectableOptions( Graphics graphics, InnatePower[] innateOptions, IDrawableInnateOption[] innateGroupOptions ) {
			using Pen highlightPen = new( Color.Red, 2f );
			// Entire Innate (if this innate is an option, add its bounds)
			if(innateOptions.Contains( _power ))
				graphics.DrawRectangle( highlightPen, _layout.Bounds );
			// Selected Innate Option Group
			foreach(var layout in _layout.Options)
				if(innateGroupOptions.Contains( layout.InnateOption ))
					graphics.DrawRectangle( highlightPen, layout.Bounds );
		}

		void InitOverlayCache(Font boldFont) {
			var bounds = _layout.Bounds;
			overlayCache = new Bitmap( bounds.Width, bounds.Height );
			using var g = Graphics.FromImage( overlayCache );
			g.TranslateTransform(-bounds.X,-bounds.Y);

			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

			this.graphics = g;

			using var font = _layout.UsingRegularFont();

			if(_layout.GeneralInstructions != null)
				DrawOption(_layout.GeneralInstructions, graphics, font, boldFont );

			// Overlay area
			foreach(WrappingText_InnateOptions wrappintText in _layout.Options)
				DrawOption( wrappintText, graphics, font, boldFont );
		}

		void DrawBackgroundImage( Font boldFont ) {
			var bounds = _layout.Bounds;
			backgroundCache = new Bitmap( bounds.Width, bounds.Height );
			using var g = Graphics.FromImage( backgroundCache );
			g.TranslateTransform(-bounds.X,-bounds.Y);

			// set single-thread instance
			this.graphics = g;

			g.FillRectangle( Brushes.AliceBlue, _layout.Bounds );

			// Title
			using(var titleFont = new Font( "Arial", _layout.textEmSize, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel ))
				g.DrawString( _power.Name.ToUpper(), titleFont, Brushes.Black, _layout.TitleBounds, new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center } );

			DrawAttributeTable( g, boldFont );
		}

		#region private

		void DrawAttributeTable( Graphics graphics, Font boldFont ) {
			// Attribute Headers
			using(var titleBg = new SolidBrush( Color.FromArgb( 0xae, 0x98, 0x69 ) )) // ae9869
				graphics.FillRectangle( titleBg, _layout.AttributeRows[0] );
			var centerBoth = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			using(Font titleFont = new Font( "Arial", _layout.textEmSize * 0.8f, FontStyle.Bold, GraphicsUnit.Pixel )) {
				graphics.DrawString( "SPEED", titleFont, Brushes.White, _layout.AttributeLabelCells[0], centerBoth );
				graphics.DrawString( "RANGE", titleFont, Brushes.White, _layout.AttributeLabelCells[1], centerBoth );
				graphics.DrawString( _power.LandOrSpirit == LandOrSpirit.Land ? "TARGET LAND" : "TARGET", titleFont, Brushes.White, _layout.AttributeLabelCells[2], centerBoth );
			}

			// Attribute Values
			graphics.FillRectangle( Brushes.BlanchedAlmond, _layout.AttributeRows[1] );
			foreach(var valueRect in _layout.AttributeValueCells)
				graphics.DrawRectangle( Pens.Black, valueRect );

			imageDrawer.DrawFitHeight(
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

		void DrawOption(WrappingText data, Graphics graphics, Font regFont, Font boldFont ) {

			foreach(var tp in data.tokens)
				imageDrawer.Draw( graphics, tp.TokenImg, tp.Rect );

			var verticalAlignCenter = new StringFormat{ LineAlignment = StringAlignment.Center };
			foreach(var sp in data.regularTexts)
				graphics.DrawString( sp.Text, regFont, Brushes.Black, sp.Bounds, verticalAlignCenter);

			foreach(var sp in data.boldTexts)
				graphics.DrawString( sp.Text, boldFont, Brushes.Black, sp.Bounds, verticalAlignCenter);

		}

		public void Dispose() {
			if(backgroundCache != null) {
				backgroundCache.Dispose();
				backgroundCache = null;
			}
			if(overlayCache != null) {
				overlayCache.Dispose();
				overlayCache = null;
			}
		}

		readonly InnatePower _power;
		readonly InnateLayout _layout;

		// single threaded variables...
		CachedImageDrawer imageDrawer;
		Graphics graphics;

		Bitmap backgroundCache;
		Bitmap overlayCache;

		#endregion

	}

}
