using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

internal interface IPanel {
	Rectangle Bounds { get; set; }
	void Paint( Graphics graphics );
	void OnGameLayoutChanged();
	void ActivateOptions( IDecision decision );
	Action GetClickableAction( Point clientCoords );
	void FindBounds(RegionLayoutClass regionLayout);
	int OptionCount { get; }
	RegionLayoutClass GetLayout( Rectangle bounds );
	bool HasFocus { set; }
	int ZIndex { get; }
}
