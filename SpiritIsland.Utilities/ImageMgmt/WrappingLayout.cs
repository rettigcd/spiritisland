using SpiritIsland;
using System.Drawing;

namespace SpiritIsland;

// !!! Sacred Site icons suck
// !! \r\n should causes new line( Dahan reclaim fishing grounds )

/// <summary> Generic wrap-column text layout engine </summary>
public class WrappingLayout {

	readonly ConfigWrappingLayout _config;

	#region constructor

	public WrappingLayout(
		ConfigWrappingLayout config, 
		Size rowSize,
		Graphics graphics
	) {
		_config = config;

		_imageSizeCalculator = new ImageSizeCalculator( _config.IconDimension, _config.ElementDimension );
		_maxIconHeight = System.Math.Max( _config.IconDimension, _config.ElementDimension );

		_rowSize = rowSize;
		_textCenterOffset = (int)(_rowSize.Height * .45f);

		_x = 0;
		_y = 0;

		_graphics = graphics;
	}


	#endregion

	public void AppendLine( string description, FontStyle fontStyle ) {
		Append(description,fontStyle);
		LineComplete();
	}

	public void Append( string description, FontStyle fontStyle ) {

		using var font = UsingFont( fontStyle );

		// Add or use existing
		List<TextPosition> texts;
		if(_texts.ContainsKey( fontStyle ))
			texts = _texts[fontStyle];
		else {
			texts = new List<TextPosition>();
			_texts.Add( fontStyle, texts );
		}


        string[] descriptionParts = TokenParser.Tokenize( description );

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
					int lengthThatFits = GetCharcterLengthThatFitsInWidth( currentString, font );

					if(lengthThatFits == currentString.Length) {
						// it all fits
						float fullLength = Measure( currentString, font );
						var fullLengthToken = new TextPosition( currentString, GetFullFitString( fullLength ) );
						texts.Add( fullLengthToken );
						_rowItems.Add( fullLengthToken );
						break;
					} else {
						// only part fits
						string phraseThatFits = currentString[..lengthThatFits];
						float partialLength = Measure( phraseThatFits, font );
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

	/// <summary> Skips somespace </summary>
	public void Tab( int numberOfSpaces, FontStyle style ) {
		using Font? font = UsingFont( style );
		_x += (int)Measure( new string( 'i', numberOfSpaces ), font );
	}

	#region finalize stuff

	public void FinalizeBounds() {
		if(NextLineStartingX < _x)
			LineComplete();
		_size = new Size( _rowSize.Width, _y );
	}

	/// <summary>
	/// Since the icons may overhang the row-height, this adds height 
	/// so tokens/icons don't get clipped by bounds
	/// </summary>
	public void AddIconRowOverflowHeight() {
		// Align Vertically - Center   !!! move this into .FinalizeBounds
		int verticalTokenBuffer = System.Math.Max( 0, (_maxIconHeight - _rowSize.Height) / 2 + 1 );
		IncY( verticalTokenBuffer ); // create top buffer for over hanging enlarged tokens
	}
	/// <summary>
	/// Shifts the Drawing Space by shifting everything inside of it.
	/// Called AFTER layout has been finalized and final height is known.
	/// </summary>
	/// <remarks>This is different from horizontal center alignment inside the drawing space.</remarks>
	public Point CenterDrawingSpace( Size sizeOfContainer ) {
		// Align Vertically - Center   !!! move this into .FinalizeBounds  OR Create a method Called .AlignVertically( Align.Center, ToHeight )
		int adjustX = (sizeOfContainer.Width - Size.Width) / 2;
		int adjustY = (sizeOfContainer.Height - Size.Height) / 2;
		Adjust( adjustX, adjustY );
		return new Point( adjustX, adjustY );
	}
	#endregion

	public Size Size => _size ?? throw new System.InvalidOperationException( "Size not finalized." );

	public void Adjust( int deltaX, int deltaY ) {
		foreach(var t in _tokens) { 
			t.Bounds.X+= deltaX;
			t.Bounds.Y+= deltaY;
		}
		foreach(var pair in _texts)
			foreach(TextPosition t in pair.Value) {
				t.Bounds.X+= deltaX;
				t.Bounds.Y+= deltaY;
			}
	}

	public void Paint( Graphics graphics ) {
		var layout = this;

		// Draw Tokens
		using(var imgCache = new ImgMemoryCache())
			foreach(TokenPosition tp in layout._tokens)
				graphics.DrawImage( imgCache.GetImage( tp.TokenImg ), tp.Bounds );


		// Draw Text
		using var verticalAlignCenter = new StringFormat { LineAlignment = StringAlignment.Center };
		foreach(var (fontStyle, texts) in layout._texts.Select( p => (p.Key, p.Value) ))
			using(Font font = UsingFont( fontStyle ))
				foreach(TextPosition sp in texts)
					graphics.DrawString( sp.Text, font, Brushes.Black, sp.Bounds, verticalAlignCenter );
	}

	public int LineCount { get; private set; } = 0;

	#region private

	readonly List<TokenPosition> _tokens = new();
	readonly Dictionary<FontStyle, List<TextPosition>> _texts = new();

	// Generic layout
	readonly Size _rowSize;
	readonly int _textCenterOffset;

	int Right             => _rowSize.Width;
	int RemainingWidth    => _rowSize.Width - _x;
	int NextLineStartingX => _config.Indent;

	// Moves as we add stuff
	int _x;
	int _y;

	readonly Graphics _graphics; // determining text size
	readonly ImageSizeCalculator _imageSizeCalculator;// determining icon size

	readonly List<IMoveHorizontally> _rowItems = new();

	readonly int _maxIconHeight; // the larger of the icons and the elements

	Size? _size;

	void IncY( int deltaY ) { _y += deltaY; }

	interface IMoveHorizontally { void Move( int deltaX ); }

	Font UsingFont( FontStyle style ) => new Font( FontFamily.GenericSansSerif, _config.EmSize, style, GraphicsUnit.Pixel );

	float Measure( string s, Font measuringFont ) => _graphics.MeasureString( s, measuringFont ).Width;

	int GetCharcterLengthThatFitsInWidth( string text, Font measuringFont ) {
		if(Measure( text, measuringFont ) <= RemainingWidth)
			return text.Length;

		// Start small and get longer as long as it fits
		int bestLength = 0;
		int spaceIndex = text.IndexOf( ' ' );
		while(spaceIndex != -1 && Measure( text[..spaceIndex], measuringFont ) <= RemainingWidth) {
			bestLength = spaceIndex;
			spaceIndex = text.IndexOf( ' ', spaceIndex + 1 );
		}

		return bestLength;
	}

	/// Centers vertically on line
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

	public void LineComplete() {
		// Capture Row width
		++LineCount;

		// Horizontal Alignment
		int offset = _config.HorizontalAlignment switch { Align.Center => RemainingWidth / 2, Align.Far => RemainingWidth, _ => 0 };
		foreach(IMoveHorizontally moveable in _rowItems)
			moveable.Move( offset );
		_rowItems.Clear();

		// Go to Next Line
		_x = NextLineStartingX;
		_y += _rowSize.Height;

	}

	#endregion

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

public class ConfigWrappingLayout {
	public float EmSize;
	public int ElementDimension;
	public int IconDimension;
	public Align HorizontalAlignment;
	public int Indent;
}
