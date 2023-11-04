using System.Drawing;

namespace SpiritIsland.WinForms;

interface IPaintableRect {
	Rectangle Paint(Graphics graphics,Rectangle rect);
}
