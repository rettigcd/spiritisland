namespace SpiritIsland;

/// <summary>
/// This is a Move Command
/// </summary>
/// <param name="Source"></param>
/// <param name="Destination"></param>
public record Move(ITokenLocation Source, ILocation Destination) : IOption {

	public IToken Token => Source.Token;
	public ILocation From => Source.Location;

	string IOption.Text => $"{Source.Text} => {Destination.Text}";

	public async Task<TokenMovedArgs?> Apply(int count = 1) {

		// Current implementation favors:
		//		switching token types prior to Add/Remove so events handlers don't switch token type
		//		perfoming the add/remove action After the Adding/Removing modifications

		// Possible problems to keep in mind:
		//		The token in the Added event, may be different than token that was attempted to be added.
		//		The Token in the Removed event, may be a different token than was requested to be removed.
		//		The token Added may be Different than the token Removed
		//		Move requires a special Publish because it pertains to 2 spaces - we don't want to publish it twice (once for each space)

		// Mitigating Factors
		//		The AddingToken args prevents changing the Count if it is a MoveTo

		// Remove from source
		var (removeResult, removedNotifier) = await Source.Location.SourceAsync(Source.Token, count, RemoveReason.MovedFrom);
		if( removeResult.Count == 0 ) return null;

		// Add to destination
		var (addResult, addedNotifier) = await Destination.SinkAsync(
			removeResult.Removed, // possibly modified, NOT original
			removeResult.Count, // possibly modified
			AddReason.MovedTo
		);

		// Publish
		var tokenMoved = new TokenMovedArgs(removeResult, addResult);
		await removedNotifier(tokenMoved);
		await addedNotifier(tokenMoved);
		ActionScope.Current.Log(tokenMoved);

		return tokenMoved;

	} // => Source.Token.MoveAsync(Source.Location,Destination,count);
}

static public class MoveExtensions {

	// Not Generic
	static public IEnumerable<Move> BuildMoves(this IEnumerable<SpaceToken> sources, Func<SpaceToken,IEnumerable<Space>> getMoveOptions) {
		return sources
			.SelectMany(st => getMoveOptions(st).Select(d => new Move(st,d)))
			.ToArray();
	}

	// Generic
	static public IEnumerable<Move> BuildMoves(this IEnumerable<ITokenLocation> sources, Func<ITokenLocation, IEnumerable<Space>> getMoveOptions) {
		return sources
			.SelectMany(s => getMoveOptions(s).Select(d => new Move(s, d)))
			.ToArray();
	}


	static public IEnumerable<Move> BuildMoves(this ITokenLocation source, IEnumerable<Space> destinationOptions) {
		return destinationOptions.Select(dst => new Move(source, dst));
	}
}
