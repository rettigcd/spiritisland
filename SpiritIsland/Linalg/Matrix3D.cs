namespace SpiritIsland;

/// <summary> Transforms 2D float vector where Vector is a Column Vector (on the right) </summary>
public class Matrix3D {

	// Matrix layout
	public double d00, d01, d02;     // row 1
	public double d10, d11, d12;     // row 2
	public double d20, d21, d22=1.0; // row 3

	static public Matrix3D operator *( Matrix3D rowMat, Matrix3D colMat ) {
		var row0 = rowMat.Row0; var row1 = rowMat.Row1; var row2 = rowMat.Row2;
		var col0 = colMat.Col0; var col1 = colMat.Col1; var col2 = colMat.Col2;
		return new Matrix3D {
			// left-rows * right-columns
			d00 = row0.Dot(col0), d01 = row0.Dot(col1), d02 = row0.Dot(col2),
			d10 = row1.Dot(col0), d11 = row1.Dot(col1), d12 = row1.Dot(col2),
			d20 = row2.Dot(col0), d21 = row2.Dot(col1), d22 = row2.Dot(col2),
		};
	}

	static public Matrix3D Identity => new Matrix3D { d00=1.0, d11=1.0, d22=1.0 };

	#region Multiplication helpers

	/// <summary>
	/// Helper Vector that is good for taking the dot product.
	/// </summary>
	public record struct Vector3( double X, double Y, double Z ) {
		/// <summary> Dot product </summary>
		public double Dot( Vector3 v ) => X * v.X + Y * v.Y + Z * v.Z;
	}

	public Vector3 Row0 => new Vector3( d00, d01, d02 );
	public Vector3 Row1 => new Vector3( d10, d11, d12 );
	public Vector3 Row2 => new Vector3( d20, d21, d22 );
	public Vector3 Col0 => new Vector3( d00, d10, d20 );
	public Vector3 Col1 => new Vector3( d01, d11, d21 );
	public Vector3 Col2 => new Vector3( d02, d12, d22 );

	#endregion
}

