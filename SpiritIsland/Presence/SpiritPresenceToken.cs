namespace SpiritIsland;

public class SpiritPresenceToken
	: IToken
	, ITrackMySpaces
	, IHandleTokenRemoved
	, ISerializableSpaceEntity
{

	public SpiritPresenceToken(Spirit spirit) {
		Self = spirit;
		SpaceAbreviation = Abreviate( Self.SpiritName );
	}

	string IToken.Badge => string.Empty;

	public Spirit Self { get; }

	public Task AddTo( Space space ) => space.AddAsync( this, 1 );

	public Task RemoveFrom( Space space ) => space.RemoveAsync( this, 1 );

	public string SpaceAbreviation { get; protected set; }

	#region Space Tracking

	void ITrackMySpaces.Clear() {
		_spaceCounts.Clear();
	}

	void ITrackMySpaces.TrackAdjust(Space space, int delta) {
		_spaceCounts[space] += delta;
	}

	readonly CountDictionary<Space> _spaceCounts = [];

	/// <summary> True if on any Space belonging to a Board. (Off-island holding spaces like the Destroyed track don't count.) </summary>
	public bool IsOnIsland => _spaceCounts.Keys.Any( space => space.SpaceSpec.Boards.Length != 0 );

	// public IEnumerable<Space> Spaces_Existing => _spaceCounts.Keys.Where(SpiritIsland.Space.Exists);
	public IEnumerable<Space> Spaces_Existing =>_spaceCounts.Keys.Where(ss => SpaceSpec.Exists(ss.SpaceSpec));

	/// <summary> Existing (non-statis) SppaceTokens </summary>
	public IEnumerable<SpaceToken> Deployed => this.On( Spaces_Existing );

	public bool IsOn( Board board ) => _spaceCounts.Keys.Any( space => space.SpaceSpec.Boards.Contains(board) );

	#endregion Space Tracking

	#region Token parts

	/// <summary> Used when displaying [TokenName on Space] </summary>
	string IOption.Text => SpaceAbreviation;
	Img IToken.Img => Img.Icon_Presence;
	ITokenClass IToken.Class => Self.Presence;
	public bool HasTag( ITag tag) {
		return tag == Self.Presence
			|| tag == TokenCategory.Presence;
	}

	#endregion

	#region Track Destroyed Presence

	/// <summary>
	/// Override this to add behavior that IS NOT destroyed presenced.
	/// </summary>
	public virtual async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
		if(args.Removed == this && args.Reason.IsDestroyingPresence()) {
			await Self.Presence.Destroyed.Location.SinkAsync(this,args.Count,AddReason.AddedToCard);
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

	#region Serialization

	/// <summary>
	/// Always tags as the base type, never `GetType().Name` - which concrete subclass this actually is
	/// gets decided entirely by "which Spirit subtype + which aspect was applied," both deterministically
	/// replayed by whatever reconstructs the Spirit before RestoreFromJson runs (same "replay setup"
	/// reasoning as GrowthTrack/InnatePowers, section 2) - so identity only needs to say *which spirit*,
	/// never *which subclass*. Subclasses with real extra state beyond `Self` (e.g. `BlisteringHeat`'s
	/// `_downgradedTokens`, `FrightfulShadowsEludeDestruction`'s `UsedThisRound`) still override this to
	/// append their own state, but no longer need a distinct tag/registration just to identify their type.
	/// </summary>
	public virtual JsonArray ToJson( ISerializationContext ctx ) => new JsonArray( nameof( SpiritPresenceToken ), ctx.IndexOf( Self ) );

	/// <summary>
	/// Resolved via spirit index rather than reconstructed, so this always returns the *same*
	/// SpiritPresenceToken instance a Spirit already owns (spirit.Presence.Token), not a fresh one -
	/// same reasoning/fix as Incarna.FromJson. A freshly-constructed duplicate wouldn't compare equal
	/// (no Equals/GetHashCode override - reference equality), so live code doing space[spirit.Presence.Token]
	/// would silently miss a deserialized token that wasn't this same instance.
	/// </summary>
	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( nameof( SpiritPresenceToken ), ( json, ctx ) => ctx.SpiritAt( (int)json[1]! ).Presence.Token );

	#endregion

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
