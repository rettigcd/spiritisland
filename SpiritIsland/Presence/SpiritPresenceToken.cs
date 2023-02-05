namespace SpiritIsland;

public class SpiritPresenceToken : IVisibleToken, TokenClass
	, ITrackMySpaces 
	, IHandleTokenRemoved
{

	public SpiritPresenceToken() {
		Text = "Presence";      // !!! this only works in SOLO.
	}

	#region Token parts

	public string Text { get; }
	Img IVisibleToken.Img => Img.Icon_Presence;
	public TokenClass Class => this;

	#endregion

	#region TokenClass parts
	string TokenClass.Label => "Presence";
	TokenCategory TokenClass.Category => TokenCategory.Presence;

	#endregion

	/// <summary>
	/// Override this to add behavior that IS NOT destroyed presenced.
	/// </summary>
	public async Task HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(args.Token == this && args.Reason.IsDestroyingPresence()) {
			Destroyed += args.Count; // no in OnPresenceDestroyed because I don't want overrides to need to call Base.
			await OnPresenceDestroyed( args );
		}
	}

	/// <summary> Override to handle DESTROYING/REMOVING/REPLACING Presence </summary>
	/// <remarks> Overrides do not need to call base, nothing is in here.</remarks>
	protected virtual Task OnPresenceDestroyed( ITokenRemovedArgs args ) => Task.CompletedTask;

	protected bool DestroysPresence( RemovingTokenArgs args ) => args.Token == this && args.Reason.IsDestroyingPresence();

	public int Destroyed { get; set; }

}
