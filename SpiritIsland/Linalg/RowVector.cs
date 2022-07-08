namespace SpiritIsland;

public record struct RowVector( float X, float Y ) {
	/// <summary> Dot product </summary>
	public static implicit operator Matrix3D.Vector3( RowVector v ) => new Matrix3D.Vector3(v.X,v.Y,1f);
	static public Matrix3D Scale( double x, double y ) => new Matrix3D { d00 = x, d11 = y };
	static public Matrix3D Translate( float x, float y ) => new Matrix3D { d00 = 1f, d11 = 1f, d20 = x, d21 = y };
	static public Matrix3D RotateDegrees( double degrees ) => Rotate( degrees * Math.PI / 180f );
	static public Matrix3D Rotate( double radians ) => new Matrix3D {
		d00 = +Math.Cos( radians ), d01 = Math.Sin( radians ),
		d10 = -Math.Sin( radians ), d11 = Math.Cos( radians ),
	};
	static public RowVector operator *( RowVector rowVector, Matrix3D colMat ) {
		Matrix3D.Vector3 row = rowVector; // convert only once
		return new RowVector( (float)(row * colMat.Col0), (float)(row * colMat.Col1) );
	}
}

