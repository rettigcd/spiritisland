using System.Drawing;

namespace SpiritIsland.WinForms;

public interface IButton {

	bool Contains( Point clientCoords );

	void Paint( Graphics graphics, bool enabled );
	
}
 