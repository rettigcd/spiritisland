namespace SpiritIsland;

public class TokenMovedArgs 
	: ITokenMovedArgs
	, ITokenAddedArgs
	, ITokenRemovedArgs
{

	public TokenMovedArgs( ITokenRemovedArgs before, ITokenAddedArgs after ) {
		// this should never happen because it is prevented in the AddingToken args
		if(before.Count != after.Count)
			throw new InvalidOperationException("Moving token should never change its count.");

		_before = before;
		_after = after;
		Count = after.Count;
	}
	readonly ITokenRemovedArgs _before;
	readonly ITokenAddedArgs _after;

	// Remove / Source / Before
	public IToken Removed => _before.Removed;
	public ILocation From     => _before.From;

	public IToken Added    => _after.Added; // might be different from Removed
	public ILocation To  => _after.To;


	public int Count { get; }

	#region Move Reasons
	RemoveReason ITokenRemovedArgs.Reason => RemoveReason.MovedFrom;
	AddReason ITokenAddedArgs.Reason => AddReason.MovedTo;
	#endregion Reasons

}

