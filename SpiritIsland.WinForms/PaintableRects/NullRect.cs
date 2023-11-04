using System.Drawing;

namespace SpiritIsland.WinForms;

class NullRect : IPaintableRect {
	public Rectangle Paint( Graphics graphics, Rectangle rect ){ /* draw nothing */ return rect; }
}
