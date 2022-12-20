using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Generic wrap-column text layout engine
/// </summary>
class WrappingLayout {

	#region layout things

	// Generic layout
	readonly Point _topLeft;
	readonly Size _rowSize;
	readonly int _textCenterOffset;

	int Right => _topLeft.X + _rowSize.Width;
	int RemainingWidth => _topLeft.X + _rowSize.Width - _x;

	// Moves as we add stuff
	int _x;
	int _y;

	readonly Graphics _graphics; // determining text size
	readonly ImageSizeCalculator _imageSizeCalculator;// determining icon size

	#endregion

	#region constructor
	public WrappingLayout( 
		Point topLeft,
		Size rowSize,
		ImageSizeCalculator imageSizeCalculator,
		Graphics graphics
	) {
		_topLeft = topLeft;
		_rowSize = rowSize;
		_textCenterOffset = (int)(_rowSize.Height * .45f);

		_x = topLeft.X;
		_y = topLeft.Y;

		_graphics = graphics;
		_imageSizeCalculator = imageSizeCalculator;
	}
	#endregion

	public Font MeasuringFont { private get; set; }
	public int Indent { private get; set; } // subsequent lines, not the 1st line


	public (List<TokenPosition>, List<TextPosition> texts) CalcWrappingString( string description ) {
		List<TokenPosition> tokens = new();
		List<TextPosition> texts = new();

		var descriptionParts = InnatePower.Tokenize( description );

		foreach(var part in descriptionParts) {
			if(part[0] == '{' && part[^1] == '}') {

				// - Token -

				var ( sz, img ) = _imageSizeCalculator.GetTokenDetails( part[1..^1] );
				tokens.Add( new TokenPosition { TokenImg = img, Rect = AddToken( sz ) } );

			} else {

				string currentString = part;
				while(0 < currentString.Length) {

					// Calculate how much of the string fits
					// (breaks on word boundaries)
					int lengthThatFits = GetCharcterLengthThatFitsInWidth( currentString );

					if(lengthThatFits == currentString.Length) {
						// it all fits
						texts.Add( new TextPosition {
							Text = currentString,
							Bounds = GetFullFitString( currentString )
						} );
						break;
					} else {
						// only part fits
						string phraseThatFits = currentString[..lengthThatFits];
						texts.Add( new TextPosition {
							Text = phraseThatFits,
							Bounds = GetPartialFitString( currentString )
						} );
						currentString = currentString[lengthThatFits..].Trim();
					}
				}

			}
		}
		return (tokens, texts);
	}

	public void Tab( int numberOfSpaces ) => _x += (int)Measure(new string('i',numberOfSpaces));

	float Measure( string s ) => _graphics.MeasureString( s, MeasuringFont ).Width;

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
	Rectangle AddToken( Size sz ) {

		// Give tokens a small left margin (if they are not at beginning of the line)
		_x += (int)(_rowSize.Height * .05f);

		// Wrap?
		bool wrap = Right < _x + sz.Width;
		if(wrap)
			GoToNextLine();

		int textHeightCenter = _y + _textCenterOffset; // 0=>draw high,  1=>draw low
		var rect = new Rectangle( _x, textHeightCenter - sz.Height / 2, sz.Width, sz.Height );

		_x += sz.Width;

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
		_x = _topLeft.X + Indent;
		_y += _rowSize.Height;
	}

	public Rectangle FinalizeBounds() {
		if( _topLeft.X + Indent < _x )
			GoToNextLine();
		return new Rectangle( _topLeft.X, _topLeft.Y, _rowSize.Width, _y-_topLeft.Y );
	}


}


