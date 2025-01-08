namespace SpiritIsland;

static public class SelectFrom {
	static public SourceSelector FromASingleLand( this SourceSelector ss ) {
		ILocation? source = null;
		ss
			.Track( spaceToken => source ??= spaceToken.Location )
			.FilterSource( space => source is null || space.Equals(source) );
		return ss;
	}
}

