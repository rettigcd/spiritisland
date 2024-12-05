namespace SpiritIsland;

/// <summary>
/// Treats XY as a Row Vector for performing 3x3 Matrix transformations.
/// XY point goes on the: Left 
/// Transformations apply: Left to Right
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
		=> radians == 0 ? Matrix3D.Identity
		: new Matrix3D {
			d00 = +Math.Cos( radians ), d01 = Math.Sin( radians ),
			d10 = -Math.Sin( radians ), d11 = Math.Cos( radians ),
		};

	#endregion static Transformation Matrix Builder

	#region static operators

	/// <summary> Applies a Matrix to a RowVector</summary>
	static public RowVector operator *( RowVector rowVector, Matrix3D colMat ) {
		Matrix3D.Vector3 row = new ( rowVector.X, rowVector.Y, 1f ); // convert only once
		return new RowVector( (float)row.Dot(colMat.Col0), (float)row.Dot(colMat.Col1) );
	}

	readonly public XY ToXY() => new XY(X, Y);

	#endregion static operators
}
