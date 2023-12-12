namespace SpiritIsland;

/// <summary>
/// 2D vector of floats that interacts with Matrices as if it were a Row.
/// </summary>
public record struct RowVector( float X, float Y ) {

	public RowVector ToUnitLength() {
		float length = Length();
		if(length == 0f)
			throw new InvalidOperationException("Cannot generate Unit Length Vector of 0-length vector.");
		float scale = 1/length;
        return new RowVector( X*scale, Y*scale );
	}

	public float LengthSquared() => X*X+Y*Y;
	public float Length() => (float)Math.Sqrt(LengthSquared());

	#region static Matrix Builder

	/// <summary> Builds a matrix that Scales RowVectors</summary>
	static public Matrix3D Scale( double x, double y ) => new Matrix3D { d00 = x, d11 = y };
	/// <summary> Builds a matrix that Translates RowVectors</summary>
	static public Matrix3D Translate( double x, double y ) => new Matrix3D { d00 = 1f, d11 = 1f, d20 = x, d21 = y };
	/// <summary> Builds a matrix that Rotates RowVectors</summary>
	static public Matrix3D RotateDegrees( double degrees ) => Rotate( degrees * Math.PI / 180f );
	/// <summary> Builds a matrix that Rotates RowVectors</summary>
	static public Matrix3D Rotate( double radians )
		=> new Matrix3D {
		d00 = +Math.Cos( radians ), d01 = Math.Sin( radians ),
		d10 = -Math.Sin( radians ), d11 = Math.Cos( radians ),
	};
	
	#endregion static Matrix Builder

	#region static operators

	/// <summary> Dot product </summary>
	static public implicit operator Matrix3D.Vector3( RowVector v ) => new Matrix3D.Vector3(v.X,v.Y,1f);

	/// <summary> Applies a Matrix to a RowVector</summary>
	static public RowVector operator *( RowVector rowVector, Matrix3D colMat ) {
		Matrix3D.Vector3 row = rowVector; // convert only once
		return new RowVector( (float)(row * colMat.Col0), (float)(row * colMat.Col1) );
	}

	static public RowVector operator *( RowVector rowVector, float scaler ) {
		return new RowVector( rowVector.X*scaler, rowVector.Y*scaler );
	}

	static public RowVector operator +( RowVector v1, RowVector v2 ) => new RowVector( v1.X+v2.X, v1.Y+v2.Y );

	static public RowVector operator -( RowVector v1, RowVector v2 ) => new RowVector( v1.X-v2.X, v1.Y-v2.Y );

	static public RowVector operator-(RowVector v) => new RowVector( -v.X, -v.Y );

	#endregion static operators
}

