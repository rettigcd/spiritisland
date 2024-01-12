namespace SpiritIsland;

public sealed class Tokens_ForIsland : IIslandTokenApi, IRunWhenTimePasses, IHaveMemento {

	public Tokens_ForIsland() {

		TokenDefaults = new Dictionary<ITokenClass, IToken> {
			[Human.City]     = new HumanToken( Human.City,     3 ),
			[Human.Town]     = new HumanToken( Human.Town,     2 ),
			[Human.Explorer] = new HumanToken( Human.Explorer, 1 ),
			[Human.Dahan]    = new HumanToken( Human.Dahan,    2 ),
		};

		_islandMods = new CountDictionary<ISpaceEntity>();
		// stick it in here so it is persisted and cleaned up during time passes
		_tokenCounts.Add( new FakeSpace( "Island-Mods" ), _islandMods );

	}

	#region Configuration

	IToken IIslandTokenApi.GetDefault( ITokenClass tokenClass ) => TokenDefaults[tokenClass];
	public readonly Dictionary<ITokenClass, IToken> TokenDefaults;

	#endregion

	readonly CountDictionary<ISpaceEntity> _islandMods;

	public void AddIslandMod( BaseModEntity token ) { ++_islandMods[token]; }

	#region IRunWhenTimePasses imp

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	Task IRunWhenTimePasses.TimePasses( GameState gameState ) {
		Dynamic.ForRound.Clear();

		foreach(var pair in _tokenCounts)
			new SpaceState( pair.Key, pair.Value, _islandMods.Keys, this ).TimePasses();

		return Task.CompletedTask;
	}

	#endregion IRunWhenTimePasses imp


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

	object IHaveMemento.Memento {
		get => new MyMemento( this );
		set {
			((MyMemento)value).Restore( this );
			Dynamic.ForRound.Clear();
		}
	}

	class MyMemento {
		public MyMemento(Tokens_ForIsland src) {
			// Save TokenCounts
			foreach(var (space,countsDict) in src._tokenCounts.Select( x => (x.Key, x.Value) ))
				_tokenCounts[space] = countsDict.Clone();
			// Save Defaults
			tokenDefaults = src.TokenDefaults.ToDictionary(p=>p.Key,p=>p.Value);
			// dynamicTokens_ForGame
			_dynamicTokens = src.Dynamic.Memento;
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
			src.Dynamic.Memento = _dynamicTokens;
		}
		readonly Dictionary<Space, CountDictionary<ISpaceEntity>> _tokenCounts = new Dictionary<Space, CountDictionary<ISpaceEntity>>();
		readonly Space[] _doesNotExist;
		readonly Dictionary<ITokenClass, IToken> tokenDefaults = new Dictionary<ITokenClass, IToken>();
		readonly object _dynamicTokens;
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
