namespace SpiritIsland;

public class TokenMovedArgs : ITokenMovedArgs, ITokenAddedArgs, ITokenRemovedArgs {

	public TokenMovedArgs( ITokenRemovedArgs before, ITokenAddedArgs after ) {
		// this should never happen because it is prevented in the AddingToken args
		if(before.Count != after.Count)
			throw new InvalidOperationException("Moving token should never change its count.");

		Before = before.Before;
		After = after.After;
		Count = after.Count;
	}

	public SpaceToken Before { get; }
	public SpaceToken After { get; }


	public IToken Removed => Before.Token;
	public Space From     => Before.Space;
	public IToken Added   => After.Token; // might be different from Removed
	public Space To       => After.Space;


	public int Count { get; }

	#region Move Reasons
	RemoveReason ITokenRemovedArgs.Reason => RemoveReason.MovedFrom;
	AddReason ITokenAddedArgs.Reason => AddReason.MovedTo;
	#endregion Reasons

}


