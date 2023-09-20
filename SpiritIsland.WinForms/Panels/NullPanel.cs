using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

public class NullPanel : IPanel {
	public Rectangle Bounds { get; set; }
	public void ActivateOptions( IDecision decision ) { }
	public Action GetClickableAction( Point clientCoords ) => null;
	public void OnGameLayoutChanged() { }
	public void Paint( Graphics graphics ) { }
	public int OptionCount => 0;
	public bool HasFocus { set { } }

	public int ZIndex => 1;

	public void FindBounds( RegionLayoutClass regionLayout ) {}
	public RegionLayoutClass GetLayout( Rectangle bounds ) { return null; }
}
