namespace SpiritIsland.JaggedEarth;

/// <remarks>
/// Creating this as a separate token from presence because:
///  - it simplifies selecting Beasts vs Presence-Beasts
///  - simplifies Moving the Presence with the Presence-Beasts into the AddedTo space.
/// </remarks>
public class ManyMindsBeast : IToken, IHandleTokenAdded, IHandleTokenRemoved {

	readonly ManyMindsPresenceToken _presenceToken;

	public ManyMindsBeast(ManyMindsPresenceToken presenceToken ) {
		_presenceToken= presenceToken;
	}

	public Img Img => Img.Beast;

	public IEntityClass Class => Token.Beast;

	public string Text => "ManyMind-SS-Beast";

	public Task HandleTokenAdded( ITokenAddedArgs args ) {
		// If we added it, it came from somewhere and represented 2 presence.
		if(args.Added == this) {
			if(args.Reason != AddReason.MovedTo) throw new InvalidOperationException($"adding MM-Beast reason {args.Reason}");
			args.To.Adjust(_presenceToken,2);
			args.To.Init( this, 1 ); // limit to max 1
		}
		return Task.CompletedTask;
	}

	public async Task HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(args.Removed != this) return;

		// Page 28 of JE says that 'Removing' presence is treated the same as Destroying, just voluntary

		var tokens = args.From;
		if(args.Reason.IsDestroyingPresence())
			await tokens.Destroy( _presenceToken, 2 );
		else if( args.Reason == RemoveReason.MovedFrom )
			await tokens.Remove( _presenceToken, 2, RemoveReason.MovedFrom);
		else
			throw new InvalidOperationException( "MM SS Beast should never be UsedUp nor .None" );

	}

}