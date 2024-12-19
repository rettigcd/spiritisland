namespace SpiritIsland;

public class Move : IOption {
	public required SpaceToken Source;
	public required Space Destination;
	string IOption.Text => $"{Source} => {Destination.Label}";
}

static public class MoveExtensions {
	static public IEnumerable<Move> BuildMoves(this IEnumerable<SpaceToken> sources, Func<SpaceToken,IEnumerable<Space>> getMoveOptions) {
		return sources
			.SelectMany(s => getMoveOptions(s).Select(d => new Move { Source = s, Destination = d }))
			.ToArray();
	}
}
