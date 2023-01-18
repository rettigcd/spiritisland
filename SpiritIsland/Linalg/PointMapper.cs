using System.Drawing;

namespace SpiritIsland;

public class PointMapper {

	/// <param name="rowVectorMatrix">Matrix must be built for transformations on RowVectors</param>
	public PointMapper(Matrix3D rowVectorMatrix) { _matrix = rowVectorMatrix; }

	public PointF Map( PointF p ) {
		RowVector result = new RowVector( p.X, p.Y ) * _matrix;
		return new PointF( result.X, result.Y );
	}

	readonly Matrix3D _matrix;
}

