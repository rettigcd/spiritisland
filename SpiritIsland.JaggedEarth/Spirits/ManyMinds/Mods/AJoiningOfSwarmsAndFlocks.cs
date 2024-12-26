namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds/Removes a single Beast token for a Sacred Site
/// </summary>
public class AJoiningOfSwarmsAndFlocks : SpiritPresenceToken, IHandleTokenAdded {

	#region Rule
	public const string Name = "A Joining of Swarms and Flocks";
	const string Description = "Your Sacred Sites may also count as beast. If something change a beast that is your presence, it affects 2 of your Presence there.";
	static public SpecialRule Rule => new SpecialRule( Name,	Description	);
	#endregion Rule

	#region constructor

	public AJoiningOfSwarmsAndFlocks(Spirit spirit):base(spirit) {
		_beastToken = new ManyMindsBeast(this);
	}

	#endregion

	Task IHandleTokenAdded.HandleTokenAddedAsync( Space to, ITokenAddedArgs args ){
		UpdateBeastCount( args.Added, to );
		return Task.CompletedTask;
	}

	public override async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ){
		await base.HandleTokenRemovedAsync(args);
		UpdateBeastCount( args.Removed, (Space)args.From );
	}

	void UpdateBeastCount( ISpaceEntity token, Space ss ) {
		if(token != this) return;
		int beastCount = ss[this] < 2 ? 0 : 1;
		ss.Init(_beastToken,beastCount);
	}

	#region private fields
	readonly ManyMindsBeast _beastToken;
	#endregion
}


/// <remarks>
/// Creating this as a separate token from presence because:
///  - it simplifies selecting Beasts vs Presence-Beasts
///  - simplifies Moving the Presence with the Presence-Beasts into the AddedTo space.
/// </remarks>
public class ManyMindsBeast(AJoiningOfSwarmsAndFlocks presenceToken)
	: IToken, IHandleTokenAdded, IHandleTokenRemoved, IAppearInSpaceAbreviation {

	readonly AJoiningOfSwarmsAndFlocks _presenceToken = presenceToken;

	public Img Img => Img.Beast;
	string IToken.Badge => "MM";

	ITokenClass IToken.Class => Token.Beast;
	bool IToken.HasTag(ITag tag) => Token.Beast.HasTag(tag);

	public string Text => "SS-Beast";

	public string SpaceAbreviation => Text;

	public Task HandleTokenAddedAsync(Space to, ITokenAddedArgs args) {
		// If we added it, it came from somewhere and represented 2 presence.
		if( args.Added == this ) {
			if( args.Reason != AddReason.MovedTo ) throw new InvalidOperationException($"adding MM-Beast reason {args.Reason}");
			to.Init(this, 1); // limit to max 1
		}
		return Task.CompletedTask;
	}

	public async Task HandleTokenRemovedAsync(ITokenRemovedArgs args) {
		if( args.Removed != this ) return;
		Space from = (Space)args.From;

		// Page 28 of JE says that 'Removing' presence is treated the same as Destroying, just voluntary

		if( args.Reason.IsDestroyingPresence() )
			await from.Destroy(_presenceToken, 2);
		else if( args is ITokenMovedArgs movedArgs && movedArgs.To is Space to ) {
			await _presenceToken.On(from).MoveTo(to, 2);
		} else
			throw new InvalidOperationException("MM SS Beast should never be UsedUp nor .None");

	}
}