using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	class InnatePainter : IDisposable {

		readonly Graphics graphics;
		readonly float width;
		readonly float iconHeight;
		readonly float elementHeight;
		readonly float rowHeight;
		readonly float textEmSize;
		Font font;
		Font boldFont;

		public InnatePainter( Graphics graphics, float width ) {
			this.width = width;
			textEmSize = width * .033f;
			iconHeight = textEmSize * 1.9f;
			elementHeight = iconHeight * 1.2f;
			rowHeight = elementHeight * 1.1f;

			// Resources
			this.graphics = graphics;
			this.font = new Font( FontFamily.GenericSansSerif, textEmSize, GraphicsUnit.Pixel );
			this.boldFont = new Font( FontFamily.GenericSansSerif, textEmSize, FontStyle.Bold, GraphicsUnit.Pixel );
		}

		public void Dispose() {
			if(font != null) {
				font.Dispose();
				font = null;
			}
			if(boldFont != null) {
				boldFont.Dispose();
				boldFont = null;
			}

			foreach(var img in images.Values)
				img.Dispose();
			images.Clear();
		}

		public void DrawFromMetrics( InnatePower power, InnateMetrics metrics, CountDictionary<Element> activatedElements, bool isActive ) {
			// graphics, Fonts, Images

			// Background
			graphics.FillRectangle( Brushes.AliceBlue, metrics.TotalInnatePowerBounds );

			// Title
			using(var titleFont = new Font( "Arial", textEmSize, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel ))
				graphics.DrawString( power.Name.ToUpper(), titleFont, Brushes.Black, metrics.TitleBounds, new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center } );

			DrawAttributeTable( power, metrics );

			// Options
			foreach(WrappingText_InnateOptions wrappintText in metrics.Options) {
				if(activatedElements.Contains( wrappintText.Elements ))
					graphics.FillRectangle( Brushes.PeachPuff, wrappintText.Bounds.ToInts() );
				//	graphics.DrawRectangle( Pens.Red, wrappintText.Bounds.ToInts() );
				wrappintText.Draw( graphics );
			}

			if(isActive) {
				using Pen highlightPen = new( Color.Red, 2f );
				graphics.DrawRectangle( highlightPen, metrics.TotalInnatePowerBounds.ToInts() );
			}

		}

		void DrawAttributeTable( InnatePower power, InnateMetrics metrics ) {
			// Attribute Headers
			using(var titleBg = new SolidBrush( Color.FromArgb( 0xae, 0x98, 0x69 ) )) // ae9869
				graphics.FillRectangle( titleBg, metrics.AttributeRows[0] );
			var centerBoth = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			using(Font titleFont = new Font( "Arial", textEmSize * 0.8f, FontStyle.Bold, GraphicsUnit.Pixel )) {
				graphics.DrawString( "SPEED", titleFont, Brushes.White, metrics.AttributeLabelCells[0], centerBoth );
				graphics.DrawString( "RANGE", titleFont, Brushes.White, metrics.AttributeLabelCells[1], centerBoth );
				graphics.DrawString( power.LandOrSpirit == LandOrSpirit.Land ? "TARGET LAND" : "TARGET", titleFont, Brushes.White, metrics.AttributeLabelCells[2], centerBoth );
			}

			// Attribute Values
			graphics.FillRectangle( Brushes.BlanchedAlmond, metrics.AttributeRows[1] );
			foreach(var valueRect in metrics.AttributeValueCells)
				graphics.DrawRectangle( Pens.Black, valueRect.ToInts() );
			graphics.DrawImageFitHeight( GetImage( power.Speed == Phase.Slow ? "slow" : "fast" ), metrics.AttributeValueCells[0].InflateBy( -metrics.AttributeValueCells[0].Height * .2f ) );
			graphics.DrawString( power.RangeText, boldFont, Brushes.Black, metrics.AttributeValueCells[1], centerBoth );
			graphics.DrawString( power.TargetFilter.ToUpper(), boldFont, Brushes.Black, metrics.AttributeValueCells[2], centerBoth );

			// Attribute Outter box
			using var thickPen = new Pen( Brushes.Black, 2f );
			graphics.DrawRectangle( thickPen, metrics.AttributeBounds.ToInts() );
		}

		public InnateMetrics CalcMetrics( InnatePower power, float x, float y ) {
			var metrics = new InnateMetrics();

			float margin = width * .02f; // 2% of width
			float workingWidth = width - margin * 2;

			// "All-Enveloping Green"
			metrics.TitleBounds = new RectangleF( x + margin, y + margin, workingWidth, workingWidth * .1f ); // 10%;				// output - titleRect

			// brown box
			metrics.AttributeBounds = new RectangleF( x + margin, metrics.TitleBounds.Bottom, workingWidth, workingWidth * .16f );
			metrics.AttributeRows = metrics.AttributeBounds.SplitVertically( 0.40f );
			metrics.AttributeLabelCells = metrics.AttributeRows[0].SplitHorizontally( 3 );
			metrics.AttributeValueCells = metrics.AttributeRows[1].SplitHorizontally( 3 );

			// Options
			var options = new List<WrappingText_InnateOptions>();
			var optionY = metrics.AttributeBounds.Bottom + rowHeight * 0.25f;
			foreach( var innatePowerOption in power.DrawableOptions ) {
				var wrapInfo = CalcInnateOptionLayout( innatePowerOption, metrics.AttributeBounds.Left, optionY, workingWidth );
				options.Add( wrapInfo );
				optionY = wrapInfo.Bounds.Bottom;
			}
			metrics.Options = options.ToArray();
			metrics.TotalInnatePowerBounds = new RectangleF( x, y, width, optionY - y );
			return metrics;
		}

		public WrappingText_InnateOptions CalcInnateOptionLayout( IDrawableInnateOption option, float originalX, float originalY, float width ) {
			float x = originalX;
			float y = originalY;

			var wrappingText = new WrappingText_InnateOptions { Elements = option.Elements };
			string elementString = option.Elements.BuildElementString();
			string description = option.Description;

			// Elements
			CalcWrappingString( wrappingText, elementString, boldFont, ref x, ref y, originalX, width );
			// Text
			CalcWrappingString( wrappingText, description, font, ref x, ref y, originalX, width );
			y += rowHeight;
			wrappingText.Bounds = new RectangleF( originalX, originalY, width, y - originalY );

			return wrappingText;
		}

		void CalcWrappingString( WrappingText_InnateOptions wrappingText, string description, Font font, ref float x, ref float y, float left, float width ) {
			var descriptionParts = InnatePower.Tokenize( description );

			var tokens = wrappingText.tokens;
			var texts = wrappingText.texts;

			foreach(var part in descriptionParts) {
				if(part[0] == '{' && part[^1] == '}') { 
					// token
					tokens.Add( CalcTokenPosition( part[1..^1], ref x, ref y, x) );
				}
				else {

					string current = part;
					while( current.Length > 0) {
						int lengthThatFits = GetCharcterLengthThatFitsInWidth( current, font, left+width-x );
						if( lengthThatFits == current.Length) {
							// it all fits
							var textSize = graphics.MeasureString(current,font);
							texts.Add( new TextPosition { Text = current, Bounds = new RectangleF(x,y,textSize.Width,rowHeight), Font = font } );
							x += textSize.Width;
							current = "";
						} else {
							// only part fits
							string subString = current.Substring(0,lengthThatFits);
							var textSize = graphics.MeasureString(current,font);
							texts.Add( new TextPosition { Text = subString, Bounds = new RectangleF(x,y,textSize.Width,rowHeight), Font = font} );
							x = left;
							y += rowHeight;
							current = current[lengthThatFits..].Trim();
						}
					}

				}
			}

		}

		int GetCharcterLengthThatFitsInWidth(string text, Font font, float width ) {
			float textWidth = graphics.MeasureString(text,font).Width;
			if(textWidth <= width)
				return text.Length;

			int bestLength = 0;
			int spaceIndex = text.IndexOf(' ');
			while(spaceIndex != -1
				&& graphics.MeasureString(text.Substring(0,spaceIndex),font).Width<=width
			) {
				bestLength = spaceIndex;
				spaceIndex = text.IndexOf(' ',spaceIndex+1);
			}
			return bestLength;
		}

		TokenPosition CalcTokenPosition( string token, ref float x, ref float y, float left ) {
			float height = "sun|moon|air|fire|water|plant|animal|earth".Contains( token )
				? elementHeight
				: iconHeight;

			Image img = GetImage( token );
			float width = height * img.Width / img.Height;

			// Wrap?
			if(left + this.width < x + width) { x = left; y += elementHeight; }

			var tp = new TokenPosition {
				image = img,
				Rect = new RectangleF(x, y, width, height ),
			};
			x += width;

			return tp;
		}

		private Image GetImage( string token ) {
			if(!images.ContainsKey( token ))
				LoadIcon( token );
			return images[token];
		}

		void LoadIcon( string token ) {
			string filename = token switch {
				"dahan"    => "Dahanicon",
				"city"     => "Cityicon",
				"town"     => "Townicon",
				"explorer" => "Explorericon",
				"blight"   => "Blighticon",
				"beast"    => "Beasticon",
				"fear"     => "Fearicon",
				"wilds"    => "Wildsicon",
				"fast"     => "Fasticon",
				"presence" => "Presenceicon",
				"slow"     => "Slowicon",
				"disease"  => "Diseaseicon",
				"strife"   => "Strifeicon",
				"badlands" => "Badlands",
				_          => "Simple_" + token // elements
			};
			images.Add( token, ResourceImages.Singleton.GetIcon(filename) );
		}

		readonly Dictionary<string,Image> images = new Dictionary<string, Image>();

	}

	public class TokenPosition {
		//public string token;
		public Image image;
		public RectangleF Rect;
	}

	public class TextPosition {
		public string Text;
		public Font Font;
		public RectangleF Bounds;
	}

	public class WrappingText {
		public List<TokenPosition> tokens = new List<TokenPosition>();
		public List<TextPosition> texts = new List<TextPosition>();
		public RectangleF Bounds;

		public void Draw( Graphics graphics ) {
			foreach(var tp in this.tokens)
				graphics.DrawImage( tp.image, tp.Rect  );
			var stringFormat = new StringFormat{ LineAlignment = StringAlignment.Center };
			foreach(var sp in this.texts)
				graphics.DrawString( sp.Text, sp.Font, Brushes.Black, sp.Bounds, stringFormat );
		}
	}

	public class WrappingText_InnateOptions : WrappingText {
		public CountDictionary<Element> Elements;
	}

	public class InnateMetrics {
		public RectangleF TotalInnatePowerBounds;

		public RectangleF TitleBounds; // holds the name of the innate power

		public RectangleF AttributeBounds;  // Draws Attributes box
		public RectangleF[] AttributeRows;
		public RectangleF[] AttributeLabelCells;
		public RectangleF[] AttributeValueCells;

		public WrappingText_InnateOptions[] Options;
	}


}
