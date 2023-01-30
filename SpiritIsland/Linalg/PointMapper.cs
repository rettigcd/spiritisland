using System.Drawing;

namespace SpiritIsland;

public class PointMapper {

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

