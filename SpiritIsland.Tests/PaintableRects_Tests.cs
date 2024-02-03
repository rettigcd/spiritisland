using System.Drawing;

namespace SpiritIsland.Tests;

public class PaintableRects_Tests {

	[Theory]
	[InlineData(Align.Near, "","(3,20)")]  // no size
	[InlineData(Align.Near, "2","(3,20)")] // 1 fills
	[InlineData(Align.Near, "1","(3,10)")] // 1 left over
	[InlineData(Align.Near, ",.5","(3,15) (18,5)")] // 1st null, 2nd size
	[InlineData(Align.Near, ".5,","(3,5) (8,15)")] // 1st size, 2nd null
	[InlineData(Align.Near, ",1,","(3,5) (8,10) (18,5)")] // center size, outsizes split remainder
	[InlineData(Align.Near, ",2,","(3,0) (3,20) (23,0)")] // center fills, outsides get 0
	[InlineData(Align.Near, "1,2,","(3,10) (13,20) (33,0)")] // extends beyond right

	[InlineData(Align.Far, "","(3,20)")]  // no size
	[InlineData(Align.Far, "2","(3,20)")] // 1 fills
	[InlineData(Align.Far, "1","(13,10)")] // 1 left over
	[InlineData(Align.Far, ",.5","(8,15) (3,5)")] // 1st null, 2nd size
	[InlineData(Align.Far, ".5,","(18,5) (3,15)")] // 1st size, 2nd null
	[InlineData(Align.Far, ",1,","(18,5) (8,10) (3,5)")] // center size, outsizes split remainder
	[InlineData(Align.Far, ",2,","(23,0) (3,20) (3,0)")] // center fills, outsides get 0
	[InlineData(Align.Far, "1,2,","(13,10) (-7,20) (-7,0)")] // extends beyond right
	public void RowRect_3_20(Align align, string setup,string expected){
		// Given: a child with a WidthRatio
		Rect[] children = [ ..setup.Split(',').Select(Build) ];
		// When:
		new RowRect(align, children ).Paint(null,new Rectangle(3,7,20,10));
		// Then:
		string.Join(' ',children.Select(FormatXW)).ShouldBe(expected);
		foreach(var child in children)
			FormatYH(child).ShouldBe("(7,10)");
	}

	static Rect Build(string s) => new Rect( string.IsNullOrEmpty(s) ? null : float.Parse(s) );

	static string Format(Rectangle r) => $"({r.X},{r.Y},{r.Width},{r.Height})"; 
	static string Format(Rect r) => Format(r.Bounds);
	static string FormatXW(Rect r) => $"({r.Bounds.X},{r.Bounds.Width})";
	static string FormatYH(Rect r) => $"({r.Bounds.Y},{r.Bounds.Height})";

}



public class Rect : IPaintableRect {
	public Rect(float? widthRatio=null ){ WidthRatio = widthRatio; }
	public float? WidthRatio { get; set; }
	public Rectangle Bounds {get;set;}
	public void Paint( Graphics _, Rectangle bounds ){
		Bounds = bounds;
	}
}
