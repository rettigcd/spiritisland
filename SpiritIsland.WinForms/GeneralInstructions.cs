using System.Drawing;

namespace SpiritIsland.WinForms;

public class GeneralInstructions {

	public GeneralInstructions( string description, float textEmSize, Size rowSize, Point topLeft ) {
		_description = description;
		_textEmSize = textEmSize;
		_rowSize = rowSize;
		_topLeft = topLeft;
	}

	readonly float _textEmSize;
	readonly Size _rowSize;
	readonly string _description;
	readonly Point _topLeft;

	public Rectangle Bounds => _bounds ??= CalcBounds();
	Rectangle? _bounds;

	Rectangle CalcBounds() {
		using Image img = ResourceImages.Singleton.GetGeneralInstructions( _description, _textEmSize, _rowSize );
		Size size = img.Size;
		return new Rectangle(_topLeft.X, _topLeft.Y, size.Width, size.Height);
	}

	public void Paint(Graphics g) {
		using Image img = ResourceImages.Singleton.GetGeneralInstructions( _description, _textEmSize, _rowSize );
		g.DrawImage( img, Bounds );
	}

}

