using System.Drawing;
using Shouldly;

namespace SpiritIsland.Tests;

public class PaintableRects_Tests {

	[Theory]
	[InlineData(FillFrom.Left, "","(3,20)")]  // no size
	[InlineData(FillFrom.Left, "2","(3,20)")] // 1 fills
	[InlineData(FillFrom.Left, "1","(3,10)")] // 1 left over
	[InlineData(FillFrom.Left, ",.5","(3,15) (18,5)")] // 1st null, 2nd size
	[InlineData(FillFrom.Left, ".5,","(3,5) (8,15)")] // 1st size, 2nd null
	[InlineData(FillFrom.Left, ",1,","(3,5) (8,10) (18,5)")] // center size, outsizes split remainder
	[InlineData(FillFrom.Left, ",2,","(3,0) (3,20) (23,0)")] // center fills, outsides get 0
	[InlineData(FillFrom.Left, "1,2,","(3,10) (13,20) (33,0)")] // extends beyond right
	[InlineData(FillFrom.Right, "","(3,20)")]  // no size
	[InlineData(FillFrom.Right, "2","(3,20)")] // 1 fills
	[InlineData(FillFrom.Right, "1","(13,10)")] // 1 left over
	[InlineData(FillFrom.Right, ",.5","(8,15) (3,5)")] // 1st null, 2nd size
	[InlineData(FillFrom.Right, ".5,","(18,5) (3,15)")] // 1st size, 2nd null
	[InlineData(FillFrom.Right, ",1,","(18,5) (8,10) (3,5)")] // center size, outsizes split remainder
	[InlineData(FillFrom.Right, ",2,","(23,0) (3,20) (3,0)")] // center fills, outsides get 0
	[InlineData(FillFrom.Right, "1,2,","(13,10) (-7,20) (-7,0)")] // extends beyond right
	public void RowRect_3_20(FillFrom align, string widths,string expected){
		// Given: a child with a WidthRatio
		Rect[] children = [ ..widths.Split(',').Select(BuildWidth) ];
		// When:
		new RowRect(align, children ).Paint(null,new Rectangle(3,7,20,10));
		// Then:
		string.Join(' ',children.Select(FormatXW)).ShouldBe(expected);
		foreach(var child in children)
			FormatYH(child).ShouldBe("(7,10)");
	}

	[Theory]
	[InlineData(FillFrom.Top, "","(3,20)")]  // no size
	[InlineData(FillFrom.Top, "2","(3,20)")] // 1 fills
	[InlineData(FillFrom.Top, "1","(3,10)")] // 1 left over
	[InlineData(FillFrom.Top, ",.5","(3,15) (18,5)")] // 1st null, 2nd size
	[InlineData(FillFrom.Top, ".5,","(3,5) (8,15)")] // 1st size, 2nd null
	[InlineData(FillFrom.Top, ",1,","(3,5) (8,10) (18,5)")] // center size, outsizes split remainder
	[InlineData(FillFrom.Top, ",2,","(3,0) (3,20) (23,0)")] // center fills, outsides get 0
	[InlineData(FillFrom.Top, "1,2,","(3,10) (13,20) (33,0)")] // extends beyond right
	[InlineData(FillFrom.Bottom, "","(3,20)")]  // no size
	[InlineData(FillFrom.Bottom, "2","(3,20)")] // 1 fills
	[InlineData(FillFrom.Bottom, "1","(13,10)")] // 1 left over
	[InlineData(FillFrom.Bottom, ",.5","(8,15) (3,5)")] // 1st null, 2nd size
	[InlineData(FillFrom.Bottom, ".5,","(18,5) (3,15)")] // 1st size, 2nd null
	[InlineData(FillFrom.Bottom, ",1,","(18,5) (8,10) (3,5)")] // center size, outsizes split remainder
	[InlineData(FillFrom.Bottom, ",2,","(23,0) (3,20) (3,0)")] // center fills, outsides get 0
	[InlineData(FillFrom.Bottom, "1,2,","(13,10) (-7,20) (-7,0)")] // extends beyond right
	public void ColumnRect_3_20(FillFrom align, string heights, string expected){
		// Given: a child with a WidthRatio
		Rect[] children = [ ..heights.Split(',').Select(BuildHeight) ];
		// When:
		new RowRect(align, children ).Paint(null,new Rectangle(7,3,10,20));
		// Then:
		string.Join(' ',children.Select(FormatYH)).ShouldBe(expected);
		foreach(var child in children)
			FormatXW(child).ShouldBe("(7,10)");
	}


	[Fact]
	public void ColorString_ParsesName(){
		Color color = ColorString.Parse("Red");
		color.R.ShouldBe((byte)255);
		color.G.ShouldBe((byte)0);
		color.B.ShouldBe((byte)0);
		color.A.ShouldBe((byte)255);
	}

	[Fact]
	public void BadColorStrings_ThrowException(){
		Should.Throw<ArgumentException>(()=>ColorString.Parse("Red:.5"));
	}

	[Fact]
	public void PenSpec_Format(){
		PenSpec spec = "Red;3.5";
		using var mgr = spec.GetResourceMgr(new Rectangle(0,0,100,100));
#pragma warning disable CA1416 // Validate platform compatibility
		Pen pen = mgr.Resource;
		Color color = pen.Color;
		color.R.ShouldBe((byte)255);
		color.G.ShouldBe((byte)0);
		color.B.ShouldBe((byte)0);
		color.A.ShouldBe((byte)255);
#pragma warning restore CA1416 // Validate platform compatibility
	}


	static Rect BuildWidth(string s) => new Rect( string.IsNullOrEmpty(s) ? null : float.Parse(s) );
	static Rect BuildHeight(string s) => new Rect( string.IsNullOrEmpty(s) ? null : 1/float.Parse(s) );

	static string Format(Rectangle r) => $"({r.X},{r.Y},{r.Width},{r.Height})"; 
	static string Format(Rect r) => Format(r.Bounds);
	static string FormatXW(Rect r) => $"({r.Bounds.X},{r.Bounds.Width})";
	static string FormatYH(Rect r) => $"({r.Bounds.Y},{r.Bounds.Height})";

}



public class Rect( float? widthRatio = null ) : IPaintableRect {
	public float? WidthRatio { get; set; } = widthRatio; 
	public Rectangle Bounds {get;set;}
	public void Paint( Graphics _, Rectangle bounds ){
		Bounds = bounds;
	}
}
