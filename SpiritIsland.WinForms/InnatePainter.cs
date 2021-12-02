using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	class InnatePainter : IDisposable {

		readonly Graphics graphics;


		public InnatePainter( Graphics graphics ) {
			// Resources
			this.graphics = graphics;
		}

		public void DrawFromMetrics( InnatePower power, InnateLayout layout, ElementCounts activatedElements, bool isActive ) {
			// graphics, Fonts, Images

			// Background
			graphics.FillRectangle( Brushes.AliceBlue, layout.TotalInnatePowerBounds );

			// Title
			using(var titleFont = new Font( "Arial", layout.textEmSize, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel ))
				graphics.DrawString( power.Name.ToUpper(), titleFont, Brushes.Black, layout.TitleBounds, new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center } );

			using var boldFont = layout.BuildBoldFont();
			DrawAttributeTable( power, layout, boldFont );

			// Options

			using var font = layout.BuildFont();
			foreach(WrappingText_InnateOptions wrappintText in layout.Options) {
				if(activatedElements.Contains( wrappintText.Elements ))
					graphics.FillRectangle( Brushes.PeachPuff, wrappintText.Bounds );
				DrawOption( wrappintText, graphics, font, boldFont );
			}

			if(isActive) {
				using Pen highlightPen = new( Color.Red, 2f );
				graphics.DrawRectangle( highlightPen, layout.TotalInnatePowerBounds );
			}

		}

		void DrawAttributeTable( InnatePower power, InnateLayout layout, Font boldFont ) {
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
			graphics.DrawImageFitHeight( GetIcon( power.DisplaySpeed == Phase.Slow ? "slow" : "fast" ), layout.AttributeValueCells[0].InflateBy( (int)(-layout.AttributeValueCells[0].Height * .2f) ) );
			graphics.DrawString( power.RangeText, boldFont, Brushes.Black, layout.AttributeValueCells[1], centerBoth );
			graphics.DrawString( power.TargetFilter.ToUpper(), boldFont, Brushes.Black, layout.AttributeValueCells[2], centerBoth );

			// Attribute Outter box
			using var thickPen = new Pen( Brushes.Black, 2f );
			graphics.DrawRectangle( thickPen, layout.AttributeBounds );
		}

		void DrawOption(WrappingText_InnateOptions data, Graphics graphics, Font regFont, Font boldFont ) {

			foreach(var tp in data.tokens)
				graphics.DrawImage( GetIcon( tp.TokenName ), tp.Rect  );

			var verticalAlignCenter = new StringFormat{ LineAlignment = StringAlignment.Center };
			foreach(var sp in data.regularTexts)
				graphics.DrawString( sp.Text, regFont, Brushes.Black, sp.Bounds, verticalAlignCenter);

			foreach(var sp in data.boldTexts)
				graphics.DrawString( sp.Text, boldFont, Brushes.Black, sp.Bounds, verticalAlignCenter);

		}

		Image GetIcon( string iconName ) {
			if(!icons.ContainsKey( iconName ))
				icons.Add( iconName, ResourceImages.Singleton.LoadIconBySimpleName(iconName) );
			return icons[iconName];
		}
		readonly Dictionary<string,Image> icons = new Dictionary<string, Image>();

		public void Dispose() {
			foreach(var img in icons.Values)
				img.Dispose();
			icons.Clear();
		}


	}

}
