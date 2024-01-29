namespace SpiritIsland;

/// <summary>
/// Maps 1 color to a new color.
/// </summary>
public interface ColorAdjuster {
	Color GetNewColor( Color p );
}

