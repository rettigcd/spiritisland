using System.Drawing;

// Used for Spirit Panel Painter
namespace SpiritIsland.WinForms;

public class ElementLayout {
	public ElementLayout(Rectangle bounds) {
		Bounds = bounds;
		x = bounds.X;
		y = bounds.Y;
		elementSize = bounds.Height;
		step = elementSize + bounds.Height/10;
	}
	public Rectangle Bounds {get;}

	public Rectangle Rect(int index) => new Rectangle(x+step*index,y,elementSize,elementSize);

	readonly int x;
	readonly int y;
	readonly int step;
	readonly int elementSize;
}