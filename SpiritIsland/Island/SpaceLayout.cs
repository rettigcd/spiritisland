using System.Drawing;

namespace SpiritIsland;

public class SpaceLayout {

	public SpaceLayout( PointF[] corners) {
		Corners = corners;
		_bounds = CalcBounds( corners );
		Center = CalcCenterOfSpacePoints(_bounds);
	}

	public PointF[] Corners { get; } // the corners on each space.
	public PointF Center { get; private set; } // set is public for manual adjustment
	RectangleF _bounds;

	public void AdjustCenter( float deltaX, float deltaY ) {
		Center = new PointF( Center.X+deltaX, Center.Y +deltaY );
	}
	public void ReMap( PointMapper mapper ) {
		for(int i = 0; i < Corners.Length; ++i)
			Corners[i] = mapper.Map( Corners[i] );
		_bounds = CalcBounds( Corners );
		Center = CalcCenterOfSpacePoints(_bounds);
	}

	// sorted farthest from boarder to closest to boarder
	public PointF[] GetInternalGridPoints( float stepSize ) {
		Dictionary<PointF, float> distancesFromBoarder = new Dictionary<PointF, float>();

		for(float x = _bounds.Left; x <= _bounds.Right; x += stepSize)
			for(float y = _bounds.Top; y <= _bounds.Bottom; y += stepSize)
				if(Polygons.IsInside( Corners, x, y )) {
					PointF p = new PointF( x, y );
					distancesFromBoarder.Add( p, Polygons.DistanceFromPolygon(Corners,p) );
				}
		return distancesFromBoarder
			.OrderByDescending(p=>p.Value)
			.Select(p=>p.Key)
			.ToArray();
	}

	public IEnumerable<PointF> GetInternalHexPoints( float xStepSize ) {

		float yOffset = xStepSize / 2;
		float yStepSize = xStepSize * 1.2f;

		bool offset = false;
		for(float x = _bounds.Left; x <= _bounds.Right; x += xStepSize) {
			for(float y = _bounds.Top - (offset ? yOffset : 0); y <= _bounds.Bottom; y += yStepSize)
				if(Polygons.IsInside( Corners, x, y ))
					yield return new PointF( x, y );
			offset = !offset;
		}
	}

	public float DistanceFromBorder( PointF point ) => Polygons.DistanceFromPolygon( Corners, point );

	#region private
	static PointF CalcCenterOfSpacePoints(RectangleF rect ) 
		=> new PointF( rect.X + rect.Width * .5f, rect.Y + rect.Height * .5f );

	static RectangleF CalcBounds(PointF[] corners) {
		float maxX = -1000f, maxY = -1000f, minX = 1000f, minY = 1000f;
		foreach(var p in corners) {
			if(p.X < minX) minX = p.X;
			if(p.Y < minY) minY = p.Y;
			if(p.X > maxX) maxX = p.X;
			if(p.Y > maxY) maxY = p.Y;
		}
		var bounds = new RectangleF( minX, minY, maxX - minX, maxY - minY );
		return bounds;
	}
	#endregion
}
