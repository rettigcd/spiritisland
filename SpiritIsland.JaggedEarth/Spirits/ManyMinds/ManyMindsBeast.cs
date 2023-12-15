namespace SpiritIsland.JaggedEarth;

/// <remarks>
/// Creating this as a separate token from presence because:
///  - it simplifies selecting Beasts vs Presence-Beasts
///  - simplifies Moving the Presence with the Presence-Beasts into the AddedTo space.
/// </remarks>
public class ManyMindsBeast : IToken, IHandleTokenAdded, IHandleTokenRemovedAsync, IAppearInSpaceAbreviation {

	readonly ManyMindsPresenceToken _presenceToken;

	public ManyMindsBeast(ManyMindsPresenceToken presenceToken ) {
		_presenceToken= presenceToken;
	}

	public Img Img => Img.Beast;

	ITokenClass IToken.Class => Token.Beast;
	bool IToken.HasTag( ITag tag ) => Token.Beast.HasTag( tag );

	public string Text => "SS-Beast";

	public string SpaceAbreviation => Text;

	public void HandleTokenAdded( SpaceState to, ITokenAddedArgs args ) {
		// If we added it, it came from somewhere and represented 2 presence.
		if(args.Added == this) {
			if(args.Reason != AddReason.MovedTo) throw new InvalidOperationException($"adding MM-Beast reason {args.Reason}");
			to.Init( this, 1 ); // limit to max 1
		}
	}

	public async Task HandleTokenRemovedAsync( SpaceState from, ITokenRemovedArgs args ) {
		if(args.Removed != this) return;

		// Page 28 of JE says that 'Removing' presence is treated the same as Destroying, just voluntary

		if(args.Reason.IsDestroyingPresence())
			await from.Destroy( _presenceToken, 2 );
		else if( args is ITokenMovedArgs movedArgs && movedArgs.To is Space to) {
			await _presenceToken.MoveAsync(from,to,2);
		} else
			throw new InvalidOperationException( "MM SS Beast should never be UsedUp nor .None" );

	}
}