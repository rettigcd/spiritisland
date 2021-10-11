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
			this.graphics = graphics;
			this.width = width;
			//			this.font =  new Font( ResourceImages.Singleton.Fonts.Families[0], 20 );
			textEmSize = width * .033f;
			this.font = new Font( FontFamily.GenericSansSerif, textEmSize, GraphicsUnit.Pixel );
			this.boldFont = new Font( FontFamily.GenericSansSerif, textEmSize, FontStyle.Bold, GraphicsUnit.Pixel );
			this.iconHeight = textEmSize * 1.9f;
			this.elementHeight = iconHeight * 1.2f;
			this.rowHeight = elementHeight * 1.1f;
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

		public RectangleF Paint( Spirit spirit, InnatePower power, float x, float y ) {

			// === Calc Metrics ===

			float margin = width * .02f; // 2% of width
			float workingWidth = width - margin*2;
			// "All-Enveloping Green"
			var titleRect = new RectangleF( x+margin, y+margin,workingWidth,workingWidth * .1f ); // 10%;
			// brown box
			var brownBox = new RectangleF( x+margin, titleRect.Bottom, workingWidth, workingWidth * .16f);
			var brownBoxRows = brownBox.SplitVertically(0.40f);
			var headingHeaderRects = brownBoxRows[0].SplitHorizontally(3);
			var headingValueRects = brownBoxRows[1].SplitHorizontally(3);
			// Options
			List<WrappingTextInfo> options = new List<WrappingTextInfo>();
			var optionY = brownBox.Bottom + rowHeight*0.25f;
			foreach(var innatePowerOption in power.Options.Where(o=>o.Purpose != AttributePurpose.ExecuteOnly )) {
				var wrapInfo = CalcInnateOption( innatePowerOption, brownBox.Left, optionY, workingWidth );
				options.Add( wrapInfo );
				optionY=wrapInfo.Bounds.Bottom;
			}
			var totalOptionBounds = new RectangleF(x,y,width,optionY-y);


			// === Draw ===

			// Background
			graphics.FillRectangle(Brushes.AliceBlue,totalOptionBounds);

			// Title
			using(var titleFont = new Font("Arial",textEmSize,FontStyle.Bold|FontStyle.Italic, GraphicsUnit.Pixel ) )
				graphics.DrawString(power.Name.ToUpper(),titleFont,Brushes.Black,titleRect,new StringFormat{ Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });

			// Attribute Headers
			using(var titleBg = new SolidBrush( Color.FromArgb( 0xae, 0x98, 0x69 ) )) // ae9869
				graphics.FillRectangle(titleBg,brownBoxRows[0]);
			var centerBoth = new StringFormat{ Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			using( Font titleFont = new Font("Arial",textEmSize*0.8f,FontStyle.Bold, GraphicsUnit.Pixel)) {
				graphics.DrawString("SPEED"      ,titleFont,Brushes.White,headingHeaderRects[0],centerBoth);
				graphics.DrawString("RANGE"      ,titleFont,Brushes.White,headingHeaderRects[1],centerBoth);
				graphics.DrawString(power.LandOrSpirit==LandOrSpirit.Land?"TARGET LAND":"TARGET",titleFont,Brushes.White,headingHeaderRects[2],centerBoth);
			}

			// Attribute Values
			graphics.FillRectangle( Brushes.BlanchedAlmond, brownBoxRows[1] );
			foreach(var valueRect in headingValueRects)
				graphics.DrawRectangle( Pens.Black, valueRect.ToInts() );
			graphics.DrawImageFitHeight( GetImage(power.Speed == Speed.Slow ? "slow" : "fast"), headingValueRects[0].InflateBy( -headingValueRects[0].Height*.2f ) );
			graphics.DrawString( power.RangeText, boldFont, Brushes.Black, headingValueRects[1], centerBoth );
			graphics.DrawString( power.TargetFilter.ToUpper(), boldFont, Brushes.Black, headingValueRects[2], centerBoth );

			// outter box
			using(var thickPen = new Pen(Brushes.Black,2f))
				graphics.DrawRectangle(thickPen,brownBox.ToInts());

			// Options
			foreach(var o in options) {
				o.Draw( graphics );
				if( spirit.Elements.Contains( o.Attribute.ElementText ))
					graphics.DrawRectangle(Pens.Red, o.Bounds.ToInts());
			}

			return totalOptionBounds;
		}


		public WrappingTextInfo CalcInnateOption( InnateOptionAttribute option, float originalX, float originalY, float width ) {
			float x = originalX;
			float y = originalY;

			var wrappingText = new WrappingTextInfo { Attribute = option };

			CalcWrappingString( wrappingText, option.ElementText.Replace( ",", "" ), boldFont, ref x, ref y, originalX, width );
			CalcWrappingString( wrappingText, option.Description, font, ref x, ref y, originalX, width );
			y += rowHeight;
			wrappingText.Bounds = new RectangleF( originalX, originalY, width, y - originalY );

			return wrappingText;
		}

		void CalcWrappingString( WrappingTextInfo wrappingText, string description, Font font, ref float x, ref float y, float left, float width ) {
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
		public class WrappingTextInfo {
			public InnateOptionAttribute Attribute;
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
				"dahan" => "Dahanicon",
				"city" => "Cityicon",
				"town" => "Townicon",
				"explorer" => "Explorericon",
				"blight" => "Blighticon",
				"beast" => "Beasticon",
				"fear" => "Fearicon",
				"wilds" => "Wildsicon",
				"fast" => "Fasticon",
				"presence" => "Presenceicon",
				"slow" => "Slowicon",
				_ => "Simple_" + token // elements
			};
			images.Add( token, ResourceImages.Singleton.GetIcon(filename) );
		}

		readonly Dictionary<string,Image> images = new Dictionary<string, Image>();

	}

}
