namespace SpiritIsland;

/// <summary> Transforms 2D float vector where Vector is a Column Vector (on the right) </summary>
public class Matrix3D {

	// Matrix layout
	public double d00, d01, d02;     // row 1
	public double d10, d11, d12;     // row 2
	public double d20, d21, d22=1.0; // row 3

	static public Matrix3D operator *( Matrix3D rowMat, Matrix3D colMat ) {
		return new Matrix3D {
			// left-rows * right-columns
			d00 = rowMat.Row0 * colMat.Col0, d01 = rowMat.Row0 * colMat.Col1, d02 = rowMat.Row0 * colMat.Col2,
			d10 = rowMat.Row1 * colMat.Col0, d11 = rowMat.Row1 * colMat.Col1, d12 = rowMat.Row1 * colMat.Col2,
			d20 = rowMat.Row2 * colMat.Col0, d21 = rowMat.Row2 * colMat.Col1, d22 = rowMat.Row2 * colMat.Col2,
		};
	}

	#region Multiplication helpers

	public record struct Vector3( double X, double Y, double Z ) {
		/// <summary> Dot product </summary>
		public static double operator *( Vector3 a, Vector3 b ) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
	}

	public Vector3 Row0 => new Vector3( d00, d01, d02 );
	public Vector3 Row1 => new Vector3( d10, d11, d12 );
	public Vector3 Row2 => new Vector3( d20, d21, d22 );
	public Vector3 Col0 => new Vector3( d00, d10, d20 );
	public Vector3 Col1 => new Vector3( d01, d11, d21 );
	public Vector3 Col2 => new Vector3( d02, d12, d22 );

	#endregion
}

