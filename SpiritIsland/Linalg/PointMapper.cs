using System.Drawing;

namespace SpiritIsland;

public class PointMapper {

	/// <param name="m">Matrix must be built for RowVectors</param>
	public PointMapper(Matrix3D m) { this.m = m; } // identity

//	public PointMapper() { m = new Matrix3D { d00 = 1, d11 = 1, d22 = 1 }; } // identity
	//	public void Rotate( double degrees ) { m = RowVector.RotateDegrees( degrees ) * m; }
	//	public void Translate( double x, double y ) { m = RowVector.Translate( x, y ) * m; }
	//	public void Scale( double x, double y ) { m = RowVector.Scale( x, y ) * m; }

	public PointF Map( PointF p ) {
		var rowVect = new RowVector( p.X, p.Y );
		var result = rowVect * m;
		return new PointF( result.X, result.Y );
	}

	readonly Matrix3D m;
}

