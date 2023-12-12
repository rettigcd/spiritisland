namespace SpiritIsland;

public class Tokens_ForIsland : IIslandTokenApi {

	public Tokens_ForIsland( GameState gs ) {

		TokenDefaults = new Dictionary<ITokenClass, IToken> {
			[Human.City]     = new HumanToken( Human.City,     3 ),
			[Human.Town]     = new HumanToken( Human.Town,     2 ),
			[Human.Explorer] = new HumanToken( Human.Explorer, 1 ),
			[Human.Dahan]    = new HumanToken( Human.Dahan,    2 ),
			[Token.Disease]  = Token.Disease,
		};

		_islandMods = new CountDictionary<ISpaceEntity>();
		// stick it in here so it is persisted and cleaned up during time passes
		_tokenCounts.Add( new FakeSpace( "Island-Mods" ), _islandMods );

		gs.TimePasses_WholeGame += (_)=>ClearEventHandlers_ForRound();
	}

	#region Configuration

	IToken IIslandTokenApi.GetDefault( ITokenClass tokenClass ) => TokenDefaults[tokenClass];
	public readonly Dictionary<ITokenClass, IToken> TokenDefaults;

	#endregion

	readonly CountDictionary<ISpaceEntity> _islandMods;
	public void TimePasses() {
		foreach(var pair in _tokenCounts)
			new SpaceState(pair.Key,pair.Value,_islandMods.Keys,this).TimePasses();
	}

	public void AddIslandMod( BaseModEntity token ) { ++_islandMods[token]; }


	Task ClearEventHandlers_ForRound() {
		Dynamic.ForRound.Clear();
		return Task.CompletedTask;
	}

	/// <remarks>
	/// Spirit Actions should not call this directly but rather go through Space.Tokens => ActionScope.AccessTokens()
	/// UI or Test stuff that is outside of an ActionScope, may use this directly.
	/// </remarks>
	public SpaceState this[Space space] => new SpaceState( space, GetTokensCounts( space ), _islandMods.Keys, this );

	CountDictionary<ISpaceEntity> GetTokensCounts( Space key ) => _tokenCounts.Get( key, () => new CountDictionary<ISpaceEntity>() );


	public int GetDynamicTokensFor( SpaceState space, TokenClassToken token ) 
		=> Dynamic.GetTokensFor( space, token );

	public readonly DualDynamicTokens Dynamic = new DualDynamicTokens();

	#region Memento

	public virtual IMemento<Tokens_ForIsland> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<Tokens_ForIsland> memento ) { 
		((Memento)memento).Restore(this);
		ClearEventHandlers_ForRound();
	}

	protected class Memento : IMemento<Tokens_ForIsland> {
		public Memento(Tokens_ForIsland src) {
			// Save TokenCounts
			foreach(var (space,countsDict) in src._tokenCounts.Select( x => (x.Key, x.Value) ))
				_tokenCounts[space] = countsDict.Clone();
			// Save Defaults
			tokenDefaults = src.TokenDefaults.ToDictionary(p=>p.Key,p=>p.Value);
			// dynamicTokens_ForGame
			dynamicTokens = src.Dynamic.SaveToMemento();
			_doesNotExist = src._tokenCounts.Keys.Where(s=>!s.DoesExists).ToArray();
		}
		public void Restore( Tokens_ForIsland src ) {

			// Clear out the ITrackMySpaces so that .Adjust below will properly initialize
			var trackable = src._tokenCounts.Values
				.SelectMany(countDict=>countDict.Keys.OfType<ITrackMySpaces>())
				.Distinct()
				.ToArray();
			foreach(ITrackMySpaces t in trackable) t.Clear();

			// Restore TokenCounts
			src._tokenCounts.Clear();

			foreach(var space in _tokenCounts.Keys) {
				// stasis
				space.DoesExists = true; // when false, set below

				// Token counts
				SpaceState tokens = src[space];
				foreach(var (token,count) in _tokenCounts[space].Select(x=>(x.Key,x.Value))) {
					tokens.Init(token, count);
					if(tokens is ITrackMySpaces tms)
						tms.TrackAdjust(space,count);
				}
			}
			foreach(var space in _doesNotExist) space.DoesExists = false;
			// Restore Defaults
			src.TokenDefaults.Clear();
			foreach(var pair in tokenDefaults)
				src.TokenDefaults.Add(pair.Key,pair.Value);
			// Restore Dynamic tokens
			src.Dynamic.LoadFrom( dynamicTokens );
		}
		readonly Dictionary<Space, CountDictionary<ISpaceEntity>> _tokenCounts = new Dictionary<Space, CountDictionary<ISpaceEntity>>();
		readonly Space[] _doesNotExist;
		readonly Dictionary<ITokenClass, IToken> tokenDefaults = new Dictionary<ITokenClass, IToken>();
		readonly IMemento<DualDynamicTokens> dynamicTokens;
	}

	#endregion Memento

	public override string ToString() => _tokenCounts
		.Select(p=>p.Key+":"+p.Value.TokenSummary())
		.Join(" ");

	public string ToVerboseString() => _tokenCounts
		.OrderBy( p => p.Key.Label )
		.Select( p => p.Key + ":" + p.Value.TokensVerbose() )
		.Join( "\r\n" );

	readonly Dictionary<Space, CountDictionary<ISpaceEntity>> _tokenCounts = new Dictionary<Space, CountDictionary<ISpaceEntity>>();

}
