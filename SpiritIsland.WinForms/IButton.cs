using System.Drawing;

namespace SpiritIsland.WinForms;

public interface IButton {
	Rectangle Bounds { get; }
	void Paint( Graphics graphics, bool enabled );
	void SyncDataToDecision( IDecision decision );
}
