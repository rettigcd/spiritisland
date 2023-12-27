using System.Drawing;

namespace SpiritIsland.WinForms;

public interface IPaintableRect {
	Rectangle Paint(Graphics graphics,Rectangle bounds);
}

public interface IPaintableRowMember : IPaintableRect {
	/// <summary> Width / Height </summary>
	float WidthRatio {  get; }
}