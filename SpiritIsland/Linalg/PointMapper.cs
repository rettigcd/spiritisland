using System.Drawing;

namespace SpiritIsland;

public class PointMapper {

	static public PointMapper FromWorldToViewport( RectangleF worldBounds, Rectangle viewportRect ) {
		Matrix3D transform = CalcWorldToScreenMatrix( worldBounds, viewportRect );
		return new PointMapper( transform );
	}

	static Matrix3D CalcWorldToScreenMatrix( RectangleF worldRect, Rectangle viewportRect ) {
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
		_scale = (new RowVector( 1, 0 ) * _matrix) - (new RowVector( 0, 0 ) * _matrix);
	}

	public PointF Map( PointF p ) {
		RowVector result = new RowVector( p.X, p.Y ) * _matrix;
		return new PointF( result.X, result.Y );
	}

	public float XScale => _scale.X;
	public float YScale => _scale.Y;

	readonly RowVector _scale;
	readonly Matrix3D _matrix;
}

