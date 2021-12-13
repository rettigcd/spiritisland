using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SpiritIsland.WinForms {

	class InnatePainter : IDisposable {

		#region constructor

		public InnatePainter( InnatePower power, InnateLayout layout ) {
			this.power = power;
			this.layout = layout;
		}

		#endregion

		void InitOverlayCache(Font boldFont) {
			var bounds = layout.Bounds;
			overlayCache = new Bitmap( bounds.Width, bounds.Height );
			using var g = Graphics.FromImage( overlayCache );
			g.TranslateTransform(-bounds.X,-bounds.Y);

			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; // !!! add this to growth painter.

			this.graphics = g;

			// Overlay areay
			using var font = layout.BuildFont();
			foreach(WrappingText_InnateOptions wrappintText in layout.Options)
				DrawOption( wrappintText, graphics, font, boldFont );
		}

		public void DrawFromMetrics( Graphics graphics, CachedImageDrawer imageDrawer, ElementCounts activatedElements, InnatePower[] innateOptions ) {

			if(backgroundCache == null) {
				this.imageDrawer = imageDrawer;
				using var boldFont = layout.BuildBoldFont();
				DrawBackgroundImage( boldFont );
				InitOverlayCache( boldFont );
			}

			// Background Layer 
			graphics.DrawImage( backgroundCache, layout.Bounds );

			this.graphics = graphics;
			// Middle Layer - Available
			foreach(WrappingText_InnateOptions wrappintText in layout.Options)
				if(activatedElements.Contains( wrappintText.Elements ))
					graphics.FillRectangle( Brushes.PeachPuff, wrappintText.Bounds );

			// Overlay text / images
			graphics.DrawImage( overlayCache, layout.Bounds );

			// Selected
			if(innateOptions.Contains( power )) {
				using Pen highlightPen = new( Color.Red, 2f );
				graphics.DrawRectangle( highlightPen, layout.Bounds );
			}

		}

		void DrawBackgroundImage( Font boldFont ) {
			var bounds = layout.Bounds;
			backgroundCache = new Bitmap( bounds.Width, bounds.Height );
			using var g = Graphics.FromImage( backgroundCache );
			g.TranslateTransform(-bounds.X,-bounds.Y);

			// set single-thread instance
			this.graphics = g;

			g.FillRectangle( Brushes.AliceBlue, layout.Bounds );

			// Title
			using(var titleFont = new Font( "Arial", layout.textEmSize, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel ))
				g.DrawString( power.Name.ToUpper(), titleFont, Brushes.Black, layout.TitleBounds, new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center } );

			DrawAttributeTable( g, boldFont );
		}

		#region private

		void DrawAttributeTable( Graphics graphics, Font boldFont ) {
			// Attribute Headers
			using(var titleBg = new SolidBrush( Color.FromArgb( 0xae, 0x98, 0x69 ) )) // ae9869
				graphics.FillRectangle( titleBg, layout.AttributeRows[0] );
			var centerBoth = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			using(Font titleFont = new Font( "Arial", layout.textEmSize * 0.8f, FontStyle.Bold, GraphicsUnit.Pixel )) {
				graphics.DrawString( "SPEED", titleFont, Brushes.White, layout.AttributeLabelCells[0], centerBoth );
				graphics.DrawString( "RANGE", titleFont, Brushes.White, layout.AttributeLabelCells[1], centerBoth );
				graphics.DrawString( power.LandOrSpirit == LandOrSpirit.Land ? "TARGET LAND" : "TARGET", titleFont, Brushes.White, layout.AttributeLabelCells[2], centerBoth );
			}

			// Attribute Values
			graphics.FillRectangle( Brushes.BlanchedAlmond, layout.AttributeRows[1] );
			foreach(var valueRect in layout.AttributeValueCells)
				graphics.DrawRectangle( Pens.Black, valueRect );

			imageDrawer.DrawFitHeight(
				graphics,
				power.DisplaySpeed == Phase.Slow ? Img.Icon_Slow : Img.Icon_Fast,
				layout.AttributeValueCells[0].InflateBy( (int)(-layout.AttributeValueCells[0].Height * .2f) )
			);

			graphics.DrawString( power.RangeText, boldFont, Brushes.Black, layout.AttributeValueCells[1], centerBoth );
			graphics.DrawString( power.TargetFilter.ToUpper(), boldFont, Brushes.Black, layout.AttributeValueCells[2], centerBoth );

			// Attribute Outter box
			using var thickPen = new Pen( Brushes.Black, 2f );
			graphics.DrawRectangle( thickPen, layout.AttributeBounds );
		}

		void DrawOption(WrappingText_InnateOptions data, Graphics graphics, Font regFont, Font boldFont ) {

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

		readonly InnatePower power;
		readonly InnateLayout layout;

		// single threaded variables...
		CachedImageDrawer imageDrawer;
		Graphics graphics;

		Bitmap backgroundCache;
		Bitmap overlayCache;

		#endregion

	}

}
