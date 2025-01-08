namespace SpiritIsland;

static public class Move_Extension { 

	/// <summary>Removes a token from a Location</summary>
	/// <returns>null if no token removed</returns>
	static public async Task<ITokenRemovedArgs> RemoveAsync( this ILocation source, IToken token, int count=1, RemoveReason reason = RemoveReason.Removed ) {
		if( reason == RemoveReason.MovedFrom )
			throw new ArgumentException("Moving Tokens must be done from the .Move method for events to work properly",nameof(reason));

		var (removed,removedHandler) = await source.SourceAsync( token, count, reason );
		if( 0<removed.Count )
			await removedHandler(removed);

		return removed;
	}

	static public Task<ITokenRemovedArgs> RemoveAsync( this ITokenLocation tokenOn, int count=1, RemoveReason reason = RemoveReason.Removed )
		=> tokenOn.Location.RemoveAsync(tokenOn.Token,count,reason);
		
	static public Task<TokenMovedArgs?> MoveToAsync( this ITokenLocation tokenOn, ILocation destination, int count=1 )
		=> new Move(tokenOn,destination).Apply(count);
}
