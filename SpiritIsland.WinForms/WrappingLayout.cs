using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

/// <summary>
/// Generic wrap-column text layout engine
/// </summary>
public class WrappingLayout {

	#region layout things

	// Generic layout
	readonly Point _topLeft;
	readonly Size _rowSize;
	readonly int _textCenterOffset;

	int Right => _topLeft.X + _rowSize.Width;
	int RemainingWidth => _topLeft.X + _rowSize.Width - _x;

	public readonly float _textEmSize;

	// Moves as we add stuff
	int _x;
	int _y;

	readonly Graphics _graphics; // determining text size
	readonly ImageSizeCalculator _imageSizeCalculator;// determining icon size

	public readonly List<TokenPosition> _tokens = new();
	public readonly Dictionary<FontStyle, List<TextPosition>> _texts = new();

	#endregion

	Font UsingFont(FontStyle style) => new Font( FontFamily.GenericSansSerif, _textEmSize, style, GraphicsUnit.Pixel );

	#region constructor
	public WrappingLayout(
		float textEmSize,
		Size rowSize,
		Point topLeft,
		Graphics graphics
	) {
		// Font and Icon sizes
		_textEmSize = textEmSize;
		int iconDimension = (int)(_textEmSize * 1.8f);
		int elementDimension = (int)(_textEmSize * 2.4f);
		_imageSizeCalculator = new ImageSizeCalculator( iconDimension, elementDimension	);
		_maxIconHeight = System.Math.Max(iconDimension, elementDimension);

		_topLeft = topLeft;
		_rowSize = rowSize;
		_textCenterOffset = (int)(_rowSize.Height * .45f);

		_x = topLeft.X;
		_y = topLeft.Y;

		_graphics = graphics;
	}
	#endregion

	readonly int _maxIconHeight; // the larger of the icons and the elements

	/// <summary>
	/// Since the icons may overhang the row-height, this adds height 
	/// so tokens/icons don't get clipped by bounds
	/// </summary>
	public void AddIconRowOverflowHeight() {
		int verticalTokenBuffer = System.Math.Max( 0, (_maxIconHeight - _rowSize.Height) / 2 + 1 );
		IncY( verticalTokenBuffer ); // create top buffer for over hanging enlarged tokens
	}

	Font _measuringFont;
	public int Indent { private get; set; } // subsequent lines, not the 1st line

	public void CalcWrappingString( string description, FontStyle fontStyle ) {

		using var font = UsingFont( fontStyle );
		_measuringFont = font;

		// Add or us existing
		List<TextPosition> texts;
		if(_texts.ContainsKey(fontStyle))
			texts = _texts[fontStyle];
		else {
			texts = new List<TextPosition>();
			_texts.Add( fontStyle, texts );
		} 


		var descriptionParts = InnatePower.Tokenize( description );

		foreach(var part in descriptionParts) {
			if(part[0] == '{' && part[^1] == '}') {

				// - Token -

				var ( sz, img ) = _imageSizeCalculator.GetTokenDetails( part[1..^1] );
				_tokens.Add( new TokenPosition { TokenImg = img, Rect = AddToken( sz ) } );

			} else {

				string currentString = part;
				while(0 < currentString.Length) {

					// Calculate how much of the string fits
					// (breaks on word boundaries)
					int lengthThatFits = GetCharcterLengthThatFitsInWidth( currentString );

					if(lengthThatFits == currentString.Length) {
						// it all fits
						texts.Add( new TextPosition( currentString, GetFullFitString( currentString ) ) );
						break;
					} else {
						// only part fits
						string phraseThatFits = currentString[..lengthThatFits];
						texts.Add( new TextPosition( phraseThatFits, GetPartialFitString( currentString ) ) );
						currentString = currentString[lengthThatFits..].Trim();
					}
				}

			}
		}

	}

	public void Tab( int numberOfSpaces, FontStyle style ) {
		using(var font = UsingFont( style )) {
			_measuringFont = font;
			_x += (int)Measure( new string( 'i', numberOfSpaces ) );
		}
		_measuringFont = null;
	}

	float Measure( string s ) => _graphics.MeasureString( s, _measuringFont ).Width;

	int GetCharcterLengthThatFitsInWidth( string text ) {
		if(Measure( text ) <= RemainingWidth)
			return text.Length;

		// Start small and get longer as long as it fits
		int bestLength = 0;
		int spaceIndex = text.IndexOf( ' ' );
		while(spaceIndex != -1 && Measure( text[..spaceIndex] ) <= RemainingWidth) {
			bestLength = spaceIndex;
			spaceIndex = text.IndexOf( ' ', spaceIndex + 1 );
		}

		return bestLength;
	}

	RectangleF GetFullFitString( string text ) => GetFullFitString( Measure( text ) );

	RectangleF GetPartialFitString( string text ) => GetPartialFitString( Measure( text ) );

	/// <summary> Centers vertically on line </summary>
	Rectangle AddToken( Size tokenSize ) {

		// Give tokens a small left margin (if they are not at beginning of the line)
		_x += (int)(_rowSize.Height * .05f);

		// Wrap?
		bool wrap = Right < _x + tokenSize.Width;
		if(wrap)
			GoToNextLine();

		int tokenY = _y + _textCenterOffset - tokenSize.Height / 2;
		var rect = new Rectangle( _x, tokenY, tokenSize.Width, tokenSize.Height );

		_x += tokenSize.Width;

		return rect;
	}

	public RectangleF GetFullFitString( float width ) {
		var rect = new RectangleF( _x, _y, width, _rowSize.Height );
		_x += (int)width;
		return rect;
	}

	public RectangleF GetPartialFitString( float width ) {
		var b = new RectangleF( _x, _y, width, _rowSize.Height );
		GoToNextLine();
		return b;
	}
	public void GoToNextLine() {
		_x = _nextLineStartingX;
		_y += _rowSize.Height;
	}
	int _nextLineStartingX => _topLeft.X + Indent;

	public void FinalizeBounds() {
		if( _nextLineStartingX < _x )
			GoToNextLine();
		var x = new Rectangle( _topLeft.X, _topLeft.Y, _rowSize.Width, _y - _topLeft.Y );
		_bounds = x;
	}

	public Rectangle Bounds => _bounds ?? throw new System.InvalidOperationException("Bounds not finalized.");
    Rectangle? _bounds;

	public void IncY( int deltaY ) { _y += deltaY; }


	public void Paint( Graphics graphics ) {
		var layout = this;

		// Draw Tokens
		using(var imageDrawer = new CachedImageDrawer())
			foreach(TokenPosition tp in layout._tokens)
				imageDrawer.Draw( graphics, tp.TokenImg, tp.Rect );

		// Draw Text
		using var verticalAlignCenter = new StringFormat { LineAlignment = StringAlignment.Center };
		foreach(var (fontStyle, texts) in layout._texts.Select( p => (p.Key, p.Value) ))
			using(Font font = UsingFont( fontStyle ))
				foreach(TextPosition sp in texts)
					graphics.DrawString( sp.Text, font, Brushes.Black, sp.Bounds, verticalAlignCenter );
	}


}


