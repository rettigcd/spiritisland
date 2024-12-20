namespace SpiritIsland;

public record Move(SpaceToken Source, Space Destination) : IOption {
	public Task<TokenMovedArgs> Apply(int count=1) => Source.MoveTo(Destination,1);
	string IOption.Text => $"{Source} => {Destination.Label}";
}

static public class MoveExtensions {
	static public IEnumerable<Move> BuildMoves(this IEnumerable<SpaceToken> sources, Func<SpaceToken,IEnumerable<Space>> getMoveOptions) {
		return sources
			.SelectMany(s => getMoveOptions(s).Select(d => new Move(s,d)))
			.ToArray();
	}
	static public IEnumerable<Move> BuildMoves(this SpaceToken source, IEnumerable<Space> destinationOptions) {
		return destinationOptions.Select(dst => new Move(source, dst));
	}
}
