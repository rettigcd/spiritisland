namespace SpiritIsland;

public class TokenMovedArgs : ITokenMovedArgs, ITokenAddedArgs, ITokenRemovedArgs {

	public TokenMovedArgs( ITokenRemovedArgs from, ITokenAddedArgs to ) {
		if(from.Count != to.Count)
			throw new InvalidOperationException("Moving token should never change its count.");

		Removed = from.Removed;
		From    = from.From;
		Before  = from.Before;

		Added = to.Added;
		To    = to.To;
		After = to.After;

		Count = to.Count;
	}

	public IToken Removed { get; }
	public SpaceState From { get; }

	public IToken Added { get; } // might be different from removed due to Adding mods
	public SpaceState To { get; }

	public SpaceToken Before { get; }
	public SpaceToken After { get; }

	public int Count { get; }

	#region Move Reasons
	RemoveReason ITokenRemovedArgs.Reason => RemoveReason.MovedFrom;
	AddReason ITokenAddedArgs.Reason => AddReason.MovedTo;
	#endregion Reasons

}


