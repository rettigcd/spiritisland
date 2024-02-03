using System.Drawing;

namespace SpiritIsland.WinForms;

public class InnateTierBtn : IPaintableRect, IPaintAbove {

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

}
