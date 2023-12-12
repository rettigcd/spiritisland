using System.Drawing;

namespace SpiritIsland.WinForms;

public interface IButton {

	bool Contains( Point clientCoords );

	void Paint( Graphics graphics, bool enabled );
	
	/// <summary>
	/// Enables / Disables the button based on the Options available in the Decision.
	/// </summary>
	void SyncDataToDecision( IDecision decision );
}
