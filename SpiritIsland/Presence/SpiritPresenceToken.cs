namespace SpiritIsland;

public class SpiritPresenceToken : IToken, IEntityClass
	, ITrackMySpaces 
	, IHandleTokenRemovedAsync
{

	public SpiritPresenceToken(Spirit spirit) {
		_spirit = spirit;
	}

	protected readonly Spirit _spirit;


	#region Token parts

	public string Text => _spirit.Text;
	Img IToken.Img => Img.Icon_Presence;
	public IEntityClass Class => this;

	#endregion

	#region TokenClass parts
	string IEntityClass.Label => "Presence";
	TokenCategory IEntityClass.Category => TokenCategory.Presence;

	#endregion

	/// <summary>
	/// Override this to add behavior that IS NOT destroyed presenced.
	/// </summary>
	public virtual async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
		if(args.Removed == this && args.Reason.IsDestroyingPresence()) {
			Destroyed += args.Count; // no in OnPresenceDestroyed because I don't want overrides to need to call Base.
			await OnPresenceDestroyed( args );
		}
	}

	/// <summary> Override to handle DESTROYING/REMOVING/REPLACING Presence </summary>
	/// <remarks> Overrides do not need to call base, nothing is in here.</remarks>
	protected virtual Task OnPresenceDestroyed( ITokenRemovedArgs args ) => Task.CompletedTask;

	protected bool DestroysPresence( RemovingTokenArgs args ) { 
		return args.Token == this && args.Reason.IsDestroyingPresence();
	}

	public int Destroyed { get; set; }

	public bool IsOn( Board board ) => GameState.Current.Tokens.IsOn( this, board );
}
