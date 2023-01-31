using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using SpiritIsland;

namespace SpiritIsland.WinForms;

/// <summary>
/// Generic wrap-column text layout engine
/// </summary>
public class WrappingLayout {

	// !!! Sacred Site icons suck
	// !! \r\n should causes new line( Dahan reclaim fishing grounds )

	Font UsingFont( FontStyle style ) => new Font( FontFamily.GenericSansSerif, _textEmSize, style, GraphicsUnit.Pixel );

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
		_imageSizeCalculator = new ImageSizeCalculator( iconDimension, elementDimension );
		_maxIconHeight = System.Math.Max( iconDimension, elementDimension );

		_topLeft = topLeft;
		_rowSize = rowSize;
		_textCenterOffset = (int)(_rowSize.Height * .45f);

		_x = topLeft.X;
		_y = topLeft.Y;

		_graphics = graphics;
	}
	#endregion

	public int IconDimension {
		get => _imageSizeCalculator.IconDimension;
		set => _imageSizeCalculator.IconDimension = value;
	}

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
		if(_texts.ContainsKey( fontStyle ))
			texts = _texts[fontStyle];
		else {
			texts = new List<TextPosition>();
			_texts.Add( fontStyle, texts );
		}


		var descriptionParts = TokenParser.Tokenize( description );

		foreach(var part in descriptionParts) {
			if(part[0] == '{' && part[^1] == '}') {

				// - Token -
				var (sz, img) = _imageSizeCalculator.GetTokenDetails( part[1..^1] );
				AddToken( img, sz );

			} else {

				string currentString = part;
				while(0 < currentString.Length) {

					// Calculate how much of the string fits
					// (breaks on word boundaries)
					int lengthThatFits = GetCharcterLengthThatFitsInWidth( currentString );

					if(lengthThatFits == currentString.Length) {
						// it all fits
						float fullLength = Measure( currentString );
						var fullLengthToken = new TextPosition( currentString, GetFullFitString( fullLength ) );
						texts.Add( fullLengthToken );
						_rowItems.Add( fullLengthToken );
						break;
					} else {
						// only part fits
						string phraseThatFits = currentString[..lengthThatFits];
						float partialLength = Measure( phraseThatFits );
						var partialLengthToken = new TextPosition( phraseThatFits, GetPartialFitString( partialLength ) );
						texts.Add( partialLengthToken );
						_rowItems.Add( partialLengthToken );
						LineComplete();

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

	/// <summary> Centers vertically on line </summary>
	void AddToken( Img img, Size tokenSize ) {

		// If not 1st item on line, Give tokens a small left margin
		if(0 < _rowItems.Count)
			_x += (int)(_rowSize.Height * .05f);

		// Wrap?
		bool wrap = Right < _x + tokenSize.Width;
		if(wrap)
			LineComplete();

		int tokenY = _y + _textCenterOffset - tokenSize.Height / 2;
		var rect = new Rectangle( _x, tokenY, tokenSize.Width, tokenSize.Height );

		_x += tokenSize.Width;

		var imgToken = new TokenPosition( img, rect );
		_tokens.Add( imgToken );
		_rowItems.Add( imgToken );
	}

	RectangleF GetFullFitString( float width ) {
		var rect = new RectangleF( _x, _y, width, _rowSize.Height );
		_x += (int)width;
		return rect;
	}

	RectangleF GetPartialFitString( float width ) {
		var b = new RectangleF( _x, _y, width, _rowSize.Height );
		_x += (int)width; // move to end so we can calculate RemainingWidth for alignment.
		return b;
	}

	void LineComplete() {
		// Capture Row width
		++RowCount;

		// Horizontal Alignment
		int offset = HorizontalAlignment switch { WinForms.Align.Center => RemainingWidth / 2, WinForms.Align.Far => RemainingWidth, _ => 0 };
		foreach(IMoveHorizontally moveable in _rowItems)
			moveable.Move( offset );
		_rowItems.Clear();
		
		// Go to Next Line
		_x = NextLineStartingX;
		_y += _rowSize.Height;

	}

	public void FinalizeBounds() {
		if(NextLineStartingX < _x)
			LineComplete();
		var x = new Rectangle( _topLeft.X, _topLeft.Y, _rowSize.Width, _y - _topLeft.Y );
		_bounds = x;
	}

	public Rectangle Bounds => _bounds ?? throw new System.InvalidOperationException( "Bounds not finalized." );
	Rectangle? _bounds;

	public void IncY( int deltaY ) { _y += deltaY; }

	void PrivateAdjust( int deltaX, int deltaY ) {
		foreach(var t in _tokens) { 
			t.Bounds.X+= deltaX;
			t.Bounds.Y+= deltaY;
		}
		foreach(var pair in _texts)
			foreach(var t in pair.Value) {
				t.Bounds.X+= deltaX;
				t.Bounds.Y+= deltaY;
			}
		if(_bounds.HasValue) {
			Rectangle b = _bounds.Value;
			_bounds = new Rectangle( b.X+deltaX, b.Y+deltaY, b.Width, b.Height );
		}
	}

	public void Align(int width, int height) {
		// Align Vertically - Center   !!! move this into .FinalizeBounds  OR Create a method Called .AlignVertically( Align.Center, ToHeight )
		int remainingHeight = height - Bounds.Height;
		int adjustX = (width - _rowSize.Width) / 2;
		int adjustY = remainingHeight / 2;
		PrivateAdjust( adjustX, adjustY );
	}


	public void Paint( Graphics graphics ) {
		var layout = this;

		// Draw Tokens
		using(var imageDrawer = new CachedImageDrawer())
			foreach(TokenPosition tp in layout._tokens)
				imageDrawer.Draw( graphics, tp.TokenImg, tp.Bounds );

		// Draw Text
		using var verticalAlignCenter = new StringFormat { LineAlignment = StringAlignment.Center };
		foreach(var (fontStyle, texts) in layout._texts.Select( p => (p.Key, p.Value) ))
			using(Font font = UsingFont( fontStyle ))
				foreach(TextPosition sp in texts)
					graphics.DrawString( sp.Text, font, Brushes.Black, sp.Bounds, verticalAlignCenter );
	}

	#region layout things

	// Generic layout
	readonly Point _topLeft;
	readonly Size _rowSize;
	readonly int _textCenterOffset;

	int Right             => _topLeft.X + _rowSize.Width;
	int RemainingWidth    => _topLeft.X + _rowSize.Width - _x;
	int NextLineStartingX => _topLeft.X + Indent;

	// Moves as we add stuff
	int _x;
	int _y;

	readonly Graphics _graphics; // determining text size
	readonly ImageSizeCalculator _imageSizeCalculator;// determining icon size

	public int RowCount { get; private set; } = 0;
	public readonly float _textEmSize;

	public Align HorizontalAlignment = WinForms.Align.Near; // left
	public readonly List<TokenPosition> _tokens = new();
	public readonly Dictionary<FontStyle, List<TextPosition>> _texts = new();

	readonly List<IMoveHorizontally> _rowItems = new();

	#endregion

	interface IMoveHorizontally { void Move( int deltaX ); }

	public class TokenPosition : IMoveHorizontally {
		public TokenPosition( Img tokenImg, RectangleF rect ) { TokenImg = tokenImg; Bounds = rect; }
		public Img TokenImg;
		public RectangleF Bounds;
		public void Move( int deltaX ) => Bounds.X += deltaX;
	}

	public class TextPosition : IMoveHorizontally {
		public TextPosition( string text, RectangleF bounds ) { Text = text; Bounds = bounds; }
		public readonly string Text;
		public RectangleF Bounds;
		public void Move( int deltaX ) => Bounds.X += (float)deltaX;
	}

}

public static class TokenParser {
	public static string[] Tokenize( string s ) {

		var tokens = new Regex( "sacred site|destroyedpresence|presence|fast|slow"
			+ "|dahan|blight|fear|city|town|explorer"
			+ "|sun|moon|air|fire|water|plant|animal|earth"
			+ "|beast|disease|strife|wilds|badlands"
			+ "|\\+1range", RegexOptions.IgnoreCase
		).Matches( s ).Cast<Match>().ToList();

		var results = new List<string>();

		int cur = 0;
		while(cur < s.Length) {
			// no more tokens, go to the end
			if(tokens.Count == 0) {
				results.Add( s[cur..] );
				break;
			}
			var nextToken = tokens[0];
			if(nextToken.Index == cur) {
				// Add this token to the results
				results.Add( "{" + nextToken.Value.ToLower() + "}" );
				cur = nextToken.Index + nextToken.Length;
				tokens.RemoveAt( 0 );
			} else {
				// Add strings to the results
				results.Add( s[cur..nextToken.Index] );
				cur = nextToken.Index;
			}
		}
		return results.ToArray();
	}

}

