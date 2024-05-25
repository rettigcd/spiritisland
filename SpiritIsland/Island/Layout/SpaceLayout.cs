namespace SpiritIsland;

/// <summary>
/// Individual space layout based on a bounding BOARD-Rectangle of (0,0,1.5,.866)
/// </summary>
public class SpaceLayout {

	public XY[] Corners { get; }
	public XY Center { get; private set; }
	public Bounds Bounds { get; private set; }

	#region constructors
	public SpaceLayout( params XY[] corners) {
		Corners = corners;
		Bounds = BoundsBuilder.ForPoints(corners);
		Center = CalcCenterOfSpacePoints(Bounds);
	}

	#endregion constructors

	public void ReMap( PointMapper mapper ) {
		for(int i = 0; i < Corners.Length; ++i)
			Corners[i] = mapper.Map( Corners[i] );
		Bounds = BoundsBuilder.ForPoints(Corners);
		Center = CalcCenterOfSpacePoints(Bounds);
	}

	// sorted farthest from boarder to closest to boarder
	public XY[] GetInternalGridPoints( float stepSize ) {
		Dictionary<XY, float> distancesFromBoarder = [];

		for(float x = Bounds.Left; x <= Bounds.Right; x += stepSize)
			for(float y = Bounds.Top; y <= Bounds.Bottom; y += stepSize)
				if(Polygons.IsInside( Corners, x, y )) {
					XY p = new ( x, y );
					distancesFromBoarder.Add( p, Polygons.DistanceFromPolygon(Corners,p) );
				}
		return distancesFromBoarder
			.OrderByDescending(p=>p.Value)
			.Select(p=>p.Key)
			.ToArray();
	}

	public IEnumerable<XY> GetInternalHexPoints( float xStepSize ) {

		float yOffset = xStepSize / 2;
		float yStepSize = xStepSize * 1.2f;

		bool offset = false;
		for(float x = Bounds.Left; x <= Bounds.Right; x += xStepSize) {
			for(float y = Bounds.Top - (offset ? yOffset : 0); y <= Bounds.Bottom; y += yStepSize)
				if(Polygons.IsInside( Corners, x, y ))
					yield return new XY( x, y );
			offset = !offset;
		}
	}

	public float FindDistanceFromBorder( XY point ) => Polygons.DistanceFromPolygon( Corners, point );

	#region private
	static XY CalcCenterOfSpacePoints(Bounds rect)
		=> new XY( rect.X + rect.Width * .5f, rect.Y + rect.Height * .5f );

	#endregion
}
