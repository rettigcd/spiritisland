using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

internal interface IPanel {
	Rectangle Bounds { get; set; }
	void Paint( Graphics graphics );
	void OnGameLayoutChanged();
	void ActivateOptions( IDecision decision );
	Action GetClickableAction( Point clientCoords );
}
