namespace SpiritIsland;

public class SpiritPresenceToken : IToken, ITokenClass
	, ITrackMySpaces 
	, IHandleTokenRemovedAsync
	, IAppearInSpaceAbreviation
{

	public SpiritPresenceToken(Spirit spirit) {
		_spirit = spirit; 
		Text = Abreviate( _spirit.Text );
		SpaceAbreviation = Abreviate( _spirit.Text );
	}

	static string Abreviate(string words) {
		var lowercaseWords = "in,of,the,a,and,as";
		return words.Split(' ','-')
			.Select(word => {
				char k = word[0];
				if( lowercaseWords.Contains( word.ToLower() ) ) k = char.ToLower(k);
				return k.ToString();
			} )
			.Join("");
	}

	public readonly Spirit _spirit;


	public Task AddTo( SpaceState spaceState ) => spaceState.Add( this, 1 );

	public Task RemoveFrom( SpaceState spaceState ) => spaceState.Remove( this, 1 );


	#region Token parts

	/// <summary> Used when displaying [TokenName on Space] </summary>
	public string Text { get; protected set; }
	Img IToken.Img => Img.Icon_Presence;
	ITokenClass IToken.Class => this;

	#endregion

	#region TokenClass parts
	string ITokenClass.Label => "Presence";
	
	public bool HasTag( ITag tag ) => tag == this || tag == TokenCategory.Presence; // for both class and for token.

	#endregion

	/// <summary>
	/// Override this to add behavior that IS NOT destroyed presenced.
	/// </summary>
	public virtual async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
		await TrackDestroyedPresence( args );
	}

	async Task TrackDestroyedPresence( ITokenRemovedArgs args ) {
		if(args.Removed == this && args.Reason.IsDestroyingPresence()) {
			Destroyed += args.Count; // not in OnPresenceDestroyed because I don't want overrides to need to call Base.
			await OnPresenceDestroyed( args );
		}
	}

	/// <summary> Override to handle DESTROYING/REMOVING/REPLACING Presence </summary>
	/// <remarks> Overrides do not need to call base, nothing is in here.</remarks>
	protected virtual Task OnPresenceDestroyed( ITokenRemovedArgs args ) => Task.CompletedTask;

	protected bool DestroysMyPresence( RemovingTokenArgs args ) { 
		return args.Token == this && args.Reason.IsDestroyingPresence();
	}

	public int Destroyed { get; set; }

	public string SpaceAbreviation { get; protected set; }

	public bool IsOn( Board board ) => GameState.Current.Tokens.IsOn( this, board );

	public bool CanMove => _spirit.Presence.CanMove;
}
