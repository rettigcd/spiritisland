using SpiritIsland.Log;

namespace SpiritIsland;

public class TokenMovedArgs 
	: ITokenMovedArgs
	, ITokenAddedArgs
	, ITokenRemovedArgs
	, ILogEntry
{

	public IToken Removed => _before.Removed;
	public ILocation From => _before.From;

	public IToken Added => _after.Added; // might be different from Removed
	public ILocation To => _after.To;

	public int Count { get; }

	#region constructor

	public TokenMovedArgs( ITokenRemovedArgs before, ITokenAddedArgs after ) {
		// this should never happen because it is prevented in the AddingToken args
		if(before.Count != after.Count)
			throw new InvalidOperationException("Moving token should never change its count.");

		_before = before;
		_after = after;
		Count = after.Count;
	}

	#endregion

	#region ILogEntry

	public LogLevel Level => LogLevel.Debug;

	public string Msg(LogLevel level) {
		IOption from = (IOption)From;
		IOption to = (IOption)To;
		return $"{Removed.Text} moved: {from.Text} => {to.Text}";
	}

	#endregion ILogEntry

	#region Move Reasons
	RemoveReason ITokenRemovedArgs.Reason => RemoveReason.MovedFrom;
	AddReason ITokenAddedArgs.Reason => AddReason.MovedTo;
	#endregion Reasons

	#region private fields
	readonly ITokenRemovedArgs _before;
	readonly ITokenAddedArgs _after;
	#endregion private fields

}