namespace SpiritIsland;

public record struct XY(float X, float Y ) {
	readonly public float Length() => (float)Math.Sqrt( LengthSquared() );
	readonly public float LengthSquared() => X * X + Y * Y;

	readonly public XY ToUnit() {
		float length = Length();
		if(length == 0f)
			throw new InvalidOperationException( "Cannot generate Unit Length Vector of 0-length vector." );
		float scale = 1 / length;
		return new XY( X * scale, Y * scale );
	}

	readonly public float Dot(XY p) => X*p.X + Y*p.Y;

	static public XY operator *( XY v1, float scaler ) => new XY( v1.X*scaler, v1.Y*scaler );
	static public XY operator +( XY v1, XY v2 ) => new XY( v1.X+v2.X, v1.Y+v2.Y );
	static public XY operator -( XY v1, XY v2 ) => new XY( v1.X-v2.X, v1.Y-v2.Y );
	static public XY operator -( XY v) => new XY( -v.X, -v.Y );
}
