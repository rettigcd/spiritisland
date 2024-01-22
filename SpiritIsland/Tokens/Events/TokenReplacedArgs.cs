using SpiritIsland.Log;

namespace SpiritIsland;

public class TokenReplacedArgs
	: ITokenAddedArgs
	, ITokenRemovedArgs
	,ILogEntry
{
	static public TokenReplacedArgs Null( ILocation location, IToken oldToken, IToken newToken ) 
		=> new TokenReplacedArgs( location, oldToken, newToken );

	/// <summary>
	/// Generates a 0-replacement Event
	/// </summary>
	TokenReplacedArgs( ILocation location, IToken oldToken, IToken newToken ) {
		Location = location;
		RemovedCount = 0;
		Removed = oldToken ?? throw new ArgumentNullException(nameof( oldToken ) );
		AddedCount = 0;
		Added = newToken ?? throw new ArgumentNullException(nameof( newToken ) );
	}

	public TokenReplacedArgs( ITokenRemovedArgs removed, ITokenAddedArgs added ) {
		if(removed.From != added.To)
			throw new ArgumentException("Tokens replacement must happen on a singl space.");
		Location = removed.From;

		RemovedCount = removed.Count;
		Removed = removed.Removed;
		AddedCount = added.Count;
		Added = added.Added;
	}

	public int RemovedCount { get; }
	public IToken Removed { get; }
	public int AddedCount { get; }
	public IToken Added { get; }

	public ILocation Location { get; }

	#region Move Reasons
	RemoveReason ITokenRemovedArgs.Reason => RemoveReason.Replaced;
	AddReason ITokenAddedArgs.Reason => AddReason.AsReplacement;
	ILocation ITokenRemovedArgs.From => Location;
	ILocation ITokenAddedArgs.To => Location;
	int ITokenAddedArgs.Count => AddedCount;
	int ITokenRemovedArgs.Count => RemovedCount;
	#endregion Reasons

	#region ILogEntry

	LogLevel ILogEntry.Level => LogLevel.Debug;

	string ILogEntry.Msg( LogLevel level ) {
		return $"On {Location} Replaced {RemovedCount} {Removed.Text} with {AddedCount} {Added.Text}";
	}

	#endregion ILogEntry

}

