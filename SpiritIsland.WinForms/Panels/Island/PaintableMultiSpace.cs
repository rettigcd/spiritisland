using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SpiritIsland.WinForms.Panels.Island;

class PaintableMultiSpace( MultiSpace ms, SpaceLayout layout ) : PaintableSpace( ms, layout ) {

	static Color MultiSpacePerimeterColor => Color.Gold;

	protected override void DoPaintAbove( Graphics graphics ) {
		// Drawing this "above" because it is temporary.
		DrawMultiSpace( graphics, _ms );
		base.DoPaintAbove( graphics );
	}

	#region Multi-Space drawing

	public void DrawMultiSpace( Graphics graphics, MultiSpace multi ) {

		using var pen = new Pen( MultiSpacePerimeterColor, 3f );

		using var brush = UseMultiSpaceBrush( multi );

		var points = _insidePoints.SpaceLayout.Corners.Select( Mapper.Map ).Select(XY_Extensions.ToInts).ToArray();
		graphics.FillClosedCurve( brush, points, FillMode.Alternate, .25f );
		graphics.DrawClosedCurve( pen, points, .25f, FillMode.Alternate );
	}

	static LinearGradientBrush UseMultiSpaceBrush( MultiSpace multi ) {
		var brush = new LinearGradientBrush( new Rectangle( 0, 0, 30, 30 ), Color.Transparent, Color.Transparent, 45F );

		var colors = multi.OrigSpaces
			.Select( x => Color.FromArgb( 92, SpaceColor( x ) ) )
			.ToArray();

		var blend = new ColorBlend {
			Positions = new float[colors.Length * 2],
			Colors = new Color[colors.Length * 2]
		};
		float step = 1.0f / colors.Length;
		for(int i = 0; i < colors.Length; ++i) {
			blend.Positions[i * 2] = i * step;
			blend.Positions[i * 2 + 1] = (i + 1) * step;
			blend.Colors[i * 2] = blend.Colors[i * 2 + 1] = colors[i];
		}
		brush.InterpolationColors = blend;
		return brush;
	}

	static Color SpaceColor( Space space )
		=> space.IsWetland ? Color.LightBlue
		: space.IsSand ? Color.PaleGoldenrod
		: space.IsMountain ? Color.Gray
		: space.IsJungle ? Color.ForestGreen
		: space.IsOcean ? Color.Blue
		: Color.Gold;

	#endregion Multi-Space drawing

	readonly MultiSpace _ms = ms;
}