namespace SpiritIsland;

public record struct ColVector( float X, float Y ) {
	/// <summary> Dot product </summary>
	public static implicit operator Matrix3D.Vector3( ColVector v ) => new Matrix3D.Vector3( v.X, v.Y, 1f );
	static public Matrix3D Scale( double x, double y ) => new Matrix3D { d00 = x, d11 = y };
	static public Matrix3D Translate( float x, float y ) => new Matrix3D { d00 = 1f, d11 = 1f, d02 = x, d12 = y };
	static public Matrix3D RotateDegrees( double degrees ) => Rotate( degrees * Math.PI / 180f );
	static public Matrix3D Rotate( double radians ) => new Matrix3D {
		d00 = Math.Cos( radians ), d01 = -Math.Sin( radians ),
		d10 = Math.Sin( radians ), d11 = +Math.Cos( radians ),
	};
	static public ColVector operator *( Matrix3D rowMat, ColVector colVector ) {
		Matrix3D.Vector3 col = colVector; // convert only once
		return new ColVector( (float)(rowMat.Row0 * col), (float)(rowMat.Row1 * col) );
	}
}

