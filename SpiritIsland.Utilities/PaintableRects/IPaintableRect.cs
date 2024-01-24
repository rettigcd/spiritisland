using System.Drawing;

namespace SpiritIsland;

public interface IPaintableRect {
	Rectangle Paint(Graphics graphics,Rectangle bounds);
}

public interface IPaintableBlockRect : IPaintableRect {
	/// <summary> Width / Height </summary>
	float WidthRatio {  get; }
}