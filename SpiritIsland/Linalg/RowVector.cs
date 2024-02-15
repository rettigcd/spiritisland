namespace SpiritIsland;

/// <summary>
/// Treats XY as a Row Vector for performing 3x3 Matrix transformations.
/// </summary>
public record struct RowVector( float X, float Y ) {

	#region static Transformation Matrix Builder

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

	#endregion static Transformation Matrix Builder

	#region static operators

	/// <summary> Applies a Matrix to a RowVector</summary>
	static public RowVector operator *( RowVector rowVector, Matrix3D colMat ) {
		Matrix3D.Vector3 row = new ( rowVector.X, rowVector.Y, 1f ); // convert only once
		return new RowVector( (float)(row * colMat.Col0), (float)(row * colMat.Col1) );
	}

	readonly public XY ToXY() => new XY(X, Y);

	//readonly public RowVector ToUnitLength() {
	//	float length = Length();
	//	if(length == 0f)
	//		throw new InvalidOperationException("Cannot generate Unit Length Vector of 0-length vector.");
	//	float scale = 1/length;
	//	return new RowVector( X*scale, Y*scale );
	//}
	//readonly public float LengthSquared() => X*X+Y*Y;
	//readonly public float Length() => (float)Math.Sqrt(LengthSquared());
	// !!! remove thse and just use XY
	//static public RowVector operator *( RowVector rowVector, float scaler ) => new RowVector( rowVector.X*scaler, rowVector.Y*scaler );
	//static public RowVector operator +( RowVector v1, RowVector v2 ) => new RowVector( v1.X+v2.X, v1.Y+v2.Y );
	//static public RowVector operator -( RowVector v1, RowVector v2 ) => new RowVector( v1.X-v2.X, v1.Y-v2.Y );
	//static public RowVector operator -(RowVector v) => new RowVector( -v.X, -v.Y );

	#endregion static operators
}
