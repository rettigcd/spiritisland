namespace SpiritIsland;

/// <summary>
/// All Child rows have the same height.
/// </summary>
public class StackedRowRect : IPaintableRect {

	/// <summary>
	/// Constructs stacked rows that: ALL Have the same height as the max WidthRato row.
	/// </summary>
	/// <param name="rows"></param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public StackedRowRect(params IPaintableRect[] rows) {
		// At least 1 row must have a known width
		_rows = rows;
		WidthRatio = (rows.Max(r=>r.WidthRatio) ?? throw new ArgumentOutOfRangeException(nameof(rows),"at least 1 row must have a WidthRatio")) 
			/ rows.Length;
	}

	float? IPaintableRect.WidthRatio => WidthRatio;
	public float WidthRatio {get;}

	public void Paint( Graphics graphics, Rectangle bounds ){

		// All rows that are shorter than max, or unspecified, are fit to the same height

		bounds = bounds.FitBoth(WidthRatio,Align.Near);
		int rowHeight = bounds.Height/_rows.Length;
		int top=bounds.Top;
		foreach(IPaintableRect row in _rows){
			var rowRect = new Rectangle(bounds.Left,top,bounds.Width,rowHeight);
			if(row.WidthRatio.HasValue)
				rowRect = rowRect.FitBoth(row.WidthRatio.Value,Align.Near);
			row.Paint(graphics,rowRect);
			top+=rowHeight;
		}

	}

	readonly IPaintableRect[] _rows;

}