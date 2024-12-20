namespace SpiritIsland;

/// <summary>
/// This is a Move Command
/// </summary>
/// <param name="Source"></param>
/// <param name="Destination"></param>
public record Move(TokenLocation Source, Space Destination) : IOption {
	string IOption.Text => $"{Source} => {Destination.Label}";
	public Task<TokenMovedArgs?> Apply(int count = 1) => Source.Token.MoveAsync(Source.Location,Destination,count);
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
