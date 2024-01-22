namespace SpiritIsland;

public class SpiritPresenceToken 
	: IToken
	, ITrackMySpaces 
	, IHandleTokenRemovedAsync
	, IAppearInSpaceAbreviation
{

	public SpiritPresenceToken(Spirit spirit) {
		Self = spirit; 
		SpaceAbreviation = Abreviate( Self.Text );
	}

	public Spirit Self { get; }

	public Task AddTo( SpaceState spaceState ) => spaceState.AddAsync( this, 1 );

	public Task RemoveFrom( SpaceState spaceState ) => spaceState.RemoveAsync( this, 1 );

	public string SpaceAbreviation { get; protected set; }

	#region Space Tracking

	void ITrackMySpaces.Clear() {
		_spaceCounts.Clear();
		_boardCounts.Clear();
	}

	void ITrackMySpaces.TrackAdjust( Space space, int delta ) {
		_spaceCounts[space] += delta;
		foreach(var board in space.Boards)
			_boardCounts[board] += delta;
	}
	readonly CountDictionary<Space> _spaceCounts = [];
	readonly CountDictionary<Board> _boardCounts = []; // ? Is this necessary?  How many things use this?

	public bool IsOnIsland => _boardCounts.Count != 0;

	// public IEnumerable<Space> Spaces_Existing => _spaceCounts.Keys.Where(SpiritIsland.Space.Exists);
	public IEnumerable<Space> Spaces_Existing {  get {
			return _spaceCounts.Keys.Where(SpiritIsland.Space.Exists);
		} 
	}


	/// <summary> Existing (non-statis) SppaceTokens </summary>
	public IEnumerable<SpaceToken> Deployed => this.On( Spaces_Existing );

	public bool IsOn( Board board ) => 0 < _boardCounts[board];

	#endregion Space Tracking

	#region Token parts

	/// <summary> Used when displaying [TokenName on Space] </summary>
	string IOption.Text => SpaceAbreviation;
	Img IToken.Img => Img.Icon_Presence;
	ITokenClass IToken.Class => Self.Presence;
	public bool HasTag( ITag tag ) => ((ITokenClass)Self.Presence).HasTag(tag);

	#endregion

	#region Track Destroyed Presence

	/// <summary>
	/// Override this to add behavior that IS NOT destroyed presenced.
	/// </summary>
	public virtual async Task HandleTokenRemovedAsync( SpaceState from,  ITokenRemovedArgs args ) {
		if(args.Removed == this && args.Reason.IsDestroyingPresence()) {
			await Self.Presence.Destroyed.Source.SinkAsync(this,args.Count,AddReason.AddedToCard);
			await OnPresenceDestroyed( args );
		}
	}

	/// <summary> Override to handle DESTROYING/REMOVING/REPLACING Presence </summary>
	/// <remarks> Overrides do not need to call base, nothing is in here.</remarks>
	protected virtual Task OnPresenceDestroyed( ITokenRemovedArgs args ) => Task.CompletedTask;

	protected bool DestroysMyPresence( RemovingTokenArgs args ) { 
		return args.Token == this && args.Reason.IsDestroyingPresence();
	}

	#endregion Track Destroyed Presence

	#region private static helpers

	static string Abreviate(string words) {
		var lowercaseWords = "in,of,the,a,and,as";
		return words.Split(' ','-')
			.Select(word => {
				char k = word[0];
				if( lowercaseWords.Contains( word, StringComparison.CurrentCultureIgnoreCase )) k = char.ToLower(k);
				return k.ToString();
			} )
			.Join("");
	}

	#endregion
}
