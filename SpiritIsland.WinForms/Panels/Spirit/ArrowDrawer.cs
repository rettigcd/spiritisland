using System.Drawing;

namespace SpiritIsland.WinForms;

public static class ArrowDrawer {

	const float startNorm = 0.2f;
	const float endNorm = 0.8f;
	const float arrowNorm = .1f;

	static public void DrawArrow( this Graphics graphics, Pen pen, PointF from, PointF to ) {
		float dx = to.X-from.X;
		float dy = to.Y-from.Y;
		PointF newFrom = new PointF( from.X+dx*startNorm, from.Y+dy*startNorm );
		PointF newTo = new PointF( from.X + dx * endNorm, from.Y + dy * endNorm );

		float inlineX = dx * arrowNorm, inlineY = dy * arrowNorm;
		float perpX = -inlineY, perpY = inlineX;

		PointF wing1 = new PointF( newTo.X +perpX-inlineX, newTo.Y + perpY-inlineY );
		PointF wing2 = new PointF( newTo.X - perpX - inlineX, newTo.Y - perpY - inlineY );

		graphics.DrawLine( pen,newFrom,newTo );
		graphics.DrawLine( pen,newTo,wing1);
		graphics.DrawLine( pen, newTo, wing2 );
	}
}
