namespace SpiritIsland;

public class TextRect : BasePaintableRect {

	public Brush Brush { get; set; } = Brushes.Black;
	public FontSpec Font = 1f;

	public StringAlignment Vertical = StringAlignment.Center;
	public StringAlignment Horizontal = StringAlignment.Center;

	#region constructors

	public TextRect( string text ) { _text = text??string.Empty; }
	public TextRect( object obj ) { _text = obj?.ToString()??string.Empty; }
	public TextRect( Func<string> textGenerator ) { _textGenerator = textGenerator; }

	#endregion constructors

	protected override void PaintContent( Graphics graphics, Rectangle content ) {
		using ResourceMgr<Font> fontMgr = Font.GetResourceMgr(content);
		using StringFormat alignment = UseAlignment;
		string text = _text ?? _textGenerator!();
		SizeF textSize = graphics.MeasureString(text,fontMgr.Resource, new PointF(0,0), alignment);
		
		graphics.DrawString( text, fontMgr.Resource, Brush, content, alignment );
	}

	StringFormat UseAlignment => new StringFormat { 
		Alignment = Horizontal,
		LineAlignment = Vertical,
		Trimming = StringTrimming.None,
		FormatFlags = StringFormatFlags.NoWrap,
	};

	readonly string? _text;
	readonly Func<string>? _textGenerator;
}
