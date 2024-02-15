namespace SpiritIsland;

public class PointMapper {

	static public PointMapper FromWorldToViewport( Bounds worldBounds, Bounds viewportRect ) {
		Matrix3D transform = CalcWorldToScreenMatrix( worldBounds, viewportRect );
		return new PointMapper( transform );
	}

	static Matrix3D CalcWorldToScreenMatrix( Bounds worldRect, Bounds viewportRect ) {
		// calculate scaling Assuming height limited
		float scale = viewportRect.Height / worldRect.Height;

		var islandBitmapMatrix
			= RowVector.Translate( -worldRect.X, -worldRect.Y ) // translate to origin
			* RowVector.Scale( scale, -scale ) // flip-y and scale
			* RowVector.Translate( 0, viewportRect.Height ) // because 0,0 is at the bottom,left
			* RowVector.Translate( viewportRect.X, viewportRect.Y ); // translate to viewport origin
		return islandBitmapMatrix;
	}

	public readonly static PointMapper NullMapper = new PointMapper( new Matrix3D() );


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

