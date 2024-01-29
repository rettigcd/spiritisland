using System.Drawing;

namespace SpiritIsland.WinForms;

public class InnateTierBtn 
	: IPaintableRect
	, IPaintAbove
//	, IAmEnablable , IClickableLocation
{

	public float? WidthRatio => _widthRatio ??= CalcWidthRatio;
	public PaddingSpec Padding = (0,.01f);

	public InnateTierBtn( Spirit spirit, IDrawableInnateTier innateOption, SizeF relativeRowSize, ClickableContainer cc ){
		_spirit = spirit;
		_innateOption = innateOption;
		_relativeRowSize = relativeRowSize;

		cc.PaintAboves.Add(this);
	}

	public void Paint(Graphics graphics, Rectangle bounds) {
		_bounds = Padding.Content( bounds );
	}

	public void PaintAbove( Graphics graphics ) {

		// if(Enabled) {
		// 	ControlPaint.DrawButton( graphics, Bounds.InflateBy( 3 ), ButtonState.Normal );
		// 	using Pen highlightPen = new( Color.Red, 2f );
		// 	graphics.DrawRectangle( highlightPen, Bounds.InflateBy( 1 ) );
		// }

		if(_innateOption.IsActive( _spirit.Elements ))
			graphics.FillRectangle( Brushes.PeachPuff, _bounds );

		using Image img = UsingImage(_bounds.Width);
		_size = Padding.Pad(img.Size); _widthRatio=null; // size for actual bounds
		graphics.DrawImage( img, _bounds );

	}

	float CalcWidthRatio => Size.Width * 1f / Size.Height;
	Size Size => _size ??= CalcSize();
	Size CalcSize(){
		using Image img = UsingImage();
		return Padding.Pad( img.Size );
	}

	Image UsingImage( int minDesiredWidth = 0 ) => ResourceImages.Singleton.GetInnateOption( _innateOption, _relativeRowSize, minDesiredWidth );

	#region private

	Size? _size;
	float? _widthRatio;
	Rectangle _bounds;

	readonly Spirit _spirit;
	readonly IDrawableInnateTier _innateOption;
	readonly SizeF _relativeRowSize;


	#endregion

	// public bool Contains(Point point) => Bounds.Contains(point);
	// public void Click()	{
	// 	MessageBox.Show("Click!");
	// }
	//	public bool Enabled { private get; set; }
	// public InnateTierBtn SetPosition( Point topLeft, int width ) {

	// 	// Calc Actual Size
	// 	SetRowInfo(width);

	// 	using Image img = UsingImage;
	// 	Bounds = new Rectangle( topLeft.X, topLeft.Y, 
	// 		_rowInfo.Width, 
	// 		_rowInfo.Width * img.Height / img.Width
	// 	);
	// 	return this;
	// }

}
