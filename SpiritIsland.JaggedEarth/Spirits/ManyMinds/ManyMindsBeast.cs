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

	public IEntityClass Class => Token.Beast;

	public string Text => "SS-Beast";

	public string SpaceAbreviation => Text;

	public void HandleTokenAdded( ITokenAddedArgs args ) {
		// If we added it, it came from somewhere and represented 2 presence.
		if(args.Added == this) {
			if(args.Reason != AddReason.MovedTo) throw new InvalidOperationException($"adding MM-Beast reason {args.Reason}");
			args.To.Init( this, 1 ); // limit to max 1
		}
	}

	public async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
		if(args.Removed != this) return;

		// Page 28 of JE says that 'Removing' presence is treated the same as Destroying, just voluntary

		var tokens = args.From;
		if(args.Reason.IsDestroyingPresence())
			await tokens.Destroy( _presenceToken, 2 );
		else if( args is ITokenMovedArgs movedArgs) {
			await _presenceToken.Move( movedArgs.From, movedArgs.To );
			await _presenceToken.Move( movedArgs.From, movedArgs.To );
		} else
			throw new InvalidOperationException( "MM SS Beast should never be UsedUp nor .None" );

	}

}