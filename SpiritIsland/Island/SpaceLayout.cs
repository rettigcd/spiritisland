using System.Drawing;

namespace SpiritIsland;

public class SpaceLayout {

	public SpaceLayout( PointF[] corners) {
		this.corners = corners;
		Center = FindCenterOfSpacePoints();
	}

	public PointF[] corners { get; } // the corners on each space.
	public PointF Center { get; private set; } // set is public for manual adjustment

	public void AdjustCenter( float deltaX, float deltaY ) {
		Center = new PointF( Center.X+deltaX, Center.Y +deltaY );
	}
	public void ReMap( PointMapper mapper ) {
		for(int i = 0; i < corners.Length; ++i)
			corners[i] = mapper.Map( corners[i] );
		Center = FindCenterOfSpacePoints();
	}

	#region private
	PointF FindCenterOfSpacePoints() {
		float maxX = -1000f, maxY = -1000f, minX = 1000f, minY = 1000f;
		foreach(var p in corners) {
			if(p.X < minX) minX = p.X;
			if(p.Y < minY) minY = p.Y;
			if(p.X > maxX) maxX = p.X;
			if(p.Y > maxY) maxY = p.Y;
		}
		var center = new PointF( (minX + maxX) * .5f, (minY + maxY) * .5f );
		return center;
	}
	#endregion
}
