using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	public class InnateLayout {

		public Rectangle Bounds;
		public Rectangle TitleBounds;			// holds the name of the innate power
		public Rectangle AttributeBounds;		// Draws Attributes box
		public Rectangle[] AttributeRows;
		public Rectangle[] AttributeLabelCells;
		public Rectangle[] AttributeValueCells;
		public WrappingText_InnateOptions[] Options;

		#region constructor

		public InnateLayout(InnatePower power, int x, int y, int width, float textHeightMultiplier, Graphics graphics ) {
			textEmSize = width * textHeightMultiplier;

			iconDimension = (int)(textEmSize * 1.9f);
			elementHeight = (int)(textEmSize * 2.3f);
			rowHeight     = (int)(textEmSize * 2.5f);

			int margin = (int)(width * .02f); // 2% of width
			int workingWidth = width - margin * 2;

			// "All-Enveloping Green"
			TitleBounds = new Rectangle( x + margin, y + margin, workingWidth, (int)(workingWidth * .12f) ); // Height is 10% of width;				// output - titleRect

			// brown box
			AttributeBounds = new Rectangle( x + margin, TitleBounds.Bottom, workingWidth, (int)(workingWidth * .15f) );
			AttributeRows       = AttributeBounds.SplitVerticallyAt( 0.40f );
			AttributeLabelCells = AttributeRows[0].SplitHorizontally( 3 );
			AttributeValueCells = AttributeRows[1].SplitHorizontally( 3 );

			// Options
			var options = new List<WrappingText_InnateOptions>();
			int optionY = AttributeBounds.Bottom + (int)(rowHeight * 0.25f);
			foreach( var innatePowerOption in power.DrawableOptions ) {
				var wrapInfo = CalcInnateOptionLayout( innatePowerOption, AttributeBounds.Left, optionY, workingWidth, graphics );
				options.Add( wrapInfo );
				optionY = wrapInfo.Bounds.Bottom;
			}
			Options = options.ToArray();
			Bounds = new Rectangle( x, y, width, optionY - y );

		}

		#endregion

		public Font BuildFont()     => new Font( FontFamily.GenericSansSerif, textEmSize, FontStyle.Regular, GraphicsUnit.Pixel );
		public Font BuildBoldFont() => new Font( FontFamily.GenericSansSerif, textEmSize, FontStyle.Bold, GraphicsUnit.Pixel );

		#region private methods

		WrappingText_InnateOptions CalcInnateOptionLayout( IDrawableInnateOption option, int originalX, int originalY, int width, Graphics graphics ) {
			int x = originalX;
			int y = originalY;

			var wrappingText = new WrappingText_InnateOptions { Elements = option.Elements };
			string elementString = option.Elements.BuildElementString();
			string description = option.Description;

			// Elements
			using var boldFont = BuildBoldFont(); // !!
			CalcWrappingString( wrappingText.tokens, wrappingText.boldTexts, elementString, boldFont, ref x, ref y, originalX, width, graphics );
			// Text
			using var font = BuildFont(); // !!
			CalcWrappingString( wrappingText.tokens, wrappingText.regularTexts, description, font, ref x, ref y, originalX, width, graphics );
			y += rowHeight;
			wrappingText.Bounds = new Rectangle( originalX, originalY, width, y - originalY );

			return wrappingText;
		}

		void CalcWrappingString( 
			List<TokenPosition> tokens,
			List<TextPosition> texts,
			string description, 
			Font font, 
			ref int x, 
			ref int y, 
			int left, 
			int width,
			Graphics graphics
		) {
			var descriptionParts = InnatePower.Tokenize( description );

			foreach(var part in descriptionParts) {
				if(part[0] == '{' && part[^1] == '}') { 
					// token
					tokens.Add( CalcTokenPosition( part[1..^1], ref x, ref y, x, width ) );
				}
				else {

					string current = part;
					while( current.Length > 0) {
						int lengthThatFits = GetCharcterLengthThatFitsInWidth( current, font, left+width-x, graphics );
						if( lengthThatFits == current.Length) {
							// it all fits
							var textSize = graphics.MeasureString(current,font);
							texts.Add( new TextPosition { Text = current, Bounds = new RectangleF(x,y,textSize.Width,rowHeight) } );
							x += (int)textSize.Width;
							current = "";
						} else {
							// only part fits
							string subString = current[..lengthThatFits];
							var textSize = graphics.MeasureString(current,font);
							texts.Add( new TextPosition { Text = subString, Bounds = new RectangleF(x,y,textSize.Width,rowHeight) } );
							x = left;
							y += rowHeight;
							current = current[lengthThatFits..].Trim();
						}
					}

				}
			}

		}

		static int GetCharcterLengthThatFitsInWidth(string text, Font font, float width,Graphics graphics) {
			float textWidth = graphics.MeasureString(text,font).Width;
			if(textWidth <= width)
				return text.Length;

			int bestLength = 0;
			int spaceIndex = text.IndexOf(' ');
			while(spaceIndex != -1
				&& graphics.MeasureString(text[..spaceIndex],font).Width<=width
			) {
				bestLength = spaceIndex;
				spaceIndex = text.IndexOf(' ',spaceIndex+1);
			}
			return bestLength;
		}

		TokenPosition CalcTokenPosition( string tokenName, ref int x, ref int y, int left, int width ) {
			var img = SimpleWordToIcon( tokenName );
			var sz = IsElement( tokenName ) 
				? new Size( elementHeight, elementHeight )
				: InnateLayout.CalcImageSize( img, iconDimension );

			// Wrap?
			if(left + width < x + sz.Width) { x = left; y += elementHeight; }

			var tp = new TokenPosition {
				TokenImg = img,
				Rect = new Rectangle( x, y + (iconDimension - sz.Height) / 2, sz.Width, sz.Height ),
			};
			x += sz.Width;

			return tp;
		}

		static Size CalcImageSize( Img img, int iconDimension ) {
			if(!iconSizes.ContainsKey( img )) {
				using Image image = ResourceImages.Singleton.GetImage( img );
				iconSizes.Add( img, image.Size );
			}
			var sz = iconSizes[img];

			return sz.Width < sz.Height
				? new Size( iconDimension * sz.Width / sz.Height, iconDimension )
				: new Size( iconDimension, iconDimension * sz.Height / sz.Width );
		}

		private static bool IsElement( string iconName ) {
			return "sun|moon|air|fire|water|plant|animal|earth".Contains( iconName );
		}

		#endregion

		public static Img SimpleWordToIcon( string token ) {
			return token switch {
				"dahan" => Img.Icon_Dahan,
				"city" => Img.Icon_City,
				"town" => Img.Icon_Town,
				"explorer" => Img.Icon_Explorer,
				"blight" => Img.Icon_Blight,
				"beast" => Img.Icon_Beast,
				"fear" => Img.Icon_Fear,
				"wilds" => Img.Icon_Wilds,
				"fast" => Img.Icon_Fast,
				"presence" => Img.Icon_Presence,
				"slow" => Img.Icon_Slow,
				"disease" => Img.Icon_Disease,
				"strife" => Img.Icon_Strife,
				"badlands" => Img.Icon_Badlands,
				_ => ElementList.ParseEl( token ).GetIconImg(),
			};
		}


		#region temp / private fields

		public readonly float textEmSize;
		readonly int iconDimension;
		readonly int elementHeight;
		readonly int rowHeight;
		static readonly Dictionary<Img,Size> iconSizes = new Dictionary<Img, Size>();

		#endregion

	}

	public class TokenPosition {
		public Img TokenImg;
		public Rectangle Rect;
	}

	public class TextPosition {
		public string Text;
		public RectangleF Bounds;
	}

	public class WrappingText {
		public List<TokenPosition> tokens = new List<TokenPosition>();

		public List<TextPosition> boldTexts = new List<TextPosition>();

		public List<TextPosition> regularTexts = new List<TextPosition>();

		public Rectangle Bounds;

	}

	public class WrappingText_InnateOptions : WrappingText {
		public ElementCounts Elements;
	}


}
