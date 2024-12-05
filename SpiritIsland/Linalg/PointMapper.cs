namespace SpiritIsland;

public class PointMapper {

	public static PointMapper NullMapper => new PointMapper(new Matrix3D());

	static public PointMapper FromWorldToViewport( Bounds worldBounds, Bounds viewportRect, int worldRotationDegrees = 0 ) {
		Matrix3D transform = CalcWorldToScreenMatrix( worldBounds, viewportRect );
		if(worldRotationDegrees != 0 ) {
			// first rotate world coordinates
			transform = RowVector.RotateDegrees(worldRotationDegrees) * transform;
		}
		return new PointMapper( transform );
	}

	static Matrix3D CalcWorldToScreenMatrix( Bounds worldRect, Bounds viewportRect ) {
		// calculate scaling Assuming height limited
		float scale = viewportRect.Height / worldRect.Height;

		var islandBitmapMatrix
			// translate to origin
			= RowVector.Translate( -worldRect.X, -worldRect.Y )
			// scale to fit in ViewPort
			* RowVector.Scale( scale, scale )
			// flip it upside but keep it within the same range (0..Height)
			* RowVector.Scale(1, -1)
			* RowVector.Translate( 0, viewportRect.Height )
			// translate to Viewport origin
			* RowVector.Translate( viewportRect.X, viewportRect.Y );
		return islandBitmapMatrix;
	}

	static public PointMapper FitPointsInViewportHeight(Bounds viewport, IEnumerable<XY> points, params int[] allowedRotationDegrees) {

		// Init to 0-Rotation, which is always allowed
		int bestDegrees = 0;
		Bounds bestWorldBounds = BoundsBuilder.ForPoints(points);

		// Find the best angle to rotate the points so they have minumum height
		foreach( int degrees in allowedRotationDegrees ) {
			var candidatesPoints = points.Select(new PointMapper(RowVector.RotateDegrees(degrees)).Map);
			Bounds testBounds = BoundsBuilder.ForPoints(candidatesPoints);
			if( testBounds.Height < bestWorldBounds.Height ) {
				bestDegrees = degrees;
				bestWorldBounds = testBounds;
			}
		}

		// calculate scaling Assuming height limited
		float scale = viewport.Height / bestWorldBounds.Height;

		var transformationMatrix
			// rotate to the best angle
			= RowVector.RotateDegrees(bestDegrees)
			// translate to origin
			* RowVector.Translate(-bestWorldBounds.X, -bestWorldBounds.Y)
			// scale to fit in ViewPort
			* RowVector.Scale(scale, scale)
			// flip it upside but keep it within the same range (0..Height)
			* RowVector.Scale(1, -1)
			* RowVector.Translate(0, viewport.Height)
			// translate to Viewport origin
			* RowVector.Translate(viewport.X, viewport.Y);
		return new PointMapper(transformationMatrix);
	}

	/// <param name="rowVectorMatrix">Matrix must be built for transformations on RowVectors</param>
	public PointMapper(Matrix3D rowVectorMatrix) { 
		_matrix = rowVectorMatrix;
		XY p10 = (new RowVector( 1, 0 ) * _matrix).ToXY();
		XY p00 = (new RowVector( 0, 0 ) * _matrix).ToXY();
		UnitLength = (p10 - p00).Length();
	}

	public XY Map( XY p ) {
		RowVector result = new RowVector( p.X, p.Y ) * _matrix;
		return new XY( result.X, result.Y );
	}

	public float UnitLength { get; private set; }
	readonly Matrix3D _matrix;
}

