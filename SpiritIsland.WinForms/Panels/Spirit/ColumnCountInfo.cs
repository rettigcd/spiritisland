using System.Drawing;

namespace SpiritIsland.WinForms;

public class ColumnCountInfo {

	public ColumnCountInfo( int innateCount ){
		columnCount            = innateCount <= 4 ? 2 : 3; // 5 or more innates, use 3 columns
		float rowWidthToFontRatio = innateCount <= 4 ? 30f : 24f; 
		giRowSize = new SizeF( rowWidthToFontRatio, 1.5f );
		optionRowSize = new SizeF( rowWidthToFontRatio, 3f );
	}

	public readonly int columnCount;
	public readonly SizeF giRowSize;
	public readonly SizeF optionRowSize;
}
