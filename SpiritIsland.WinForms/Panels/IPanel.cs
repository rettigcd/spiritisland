using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

internal interface IPanel {
	void AssignBounds( RegionLayoutClass regionLayout );
	Rectangle Bounds { get; }

	void Paint( Graphics graphics );
	void OnGameLayoutChanged();
	void ActivateOptions( IDecision decision );

	/// <summary>
	/// Returns ONLY things that can be (logically) clicked on at the moment.
	/// </summary>
	IClickable GetClickableAction( Point clientCoords );
	
	int OptionCount { get; }
	RegionLayoutClass GetLayout( Rectangle bounds );
	bool HasFocus { set; }
	int ZIndex { get; }
}
