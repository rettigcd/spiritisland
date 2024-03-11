namespace SpiritIsland;

public sealed class Tokens_ForIsland : IIslandTokenApi, IRunWhenTimePasses, IHaveMemento {

	public Tokens_ForIsland() {

		TokenDefaults = new Dictionary<ITokenClass, HumanToken> {
			[Human.City]     = new HumanToken( Human.City,     3 ),
			[Human.Town]     = new HumanToken( Human.Town,     2 ),
			[Human.Explorer] = new HumanToken( Human.Explorer, 1 ),
			[Human.Dahan]    = new HumanToken( Human.Dahan,    2 ),
		};

		_islandMods = [];
		// stick it in here so it is persisted and cleaned up during time passes
		_tokenCounts.Add( new FakeSpace( "Island-Mods" ), _islandMods );

	}

	#region Configuration

	public HumanToken GetDefault( ITokenClass tokenClass ) => TokenDefaults[tokenClass];
	public readonly Dictionary<ITokenClass, HumanToken> TokenDefaults;

	#endregion

	readonly CountDictionary<ISpaceEntity> _islandMods;

	public void AddIslandMod( BaseModEntity token ) { ++_islandMods[token]; }

	#region IRunWhenTimePasses imp

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	Task IRunWhenTimePasses.TimePasses( GameState gameState ) {
		Dynamic.ForRound.Clear();

		foreach(var pair in _tokenCounts)
			new Space( pair.Key, pair.Value, _islandMods.Keys, this ).TimePasses();

		return Task.CompletedTask;
	}

	#endregion IRunWhenTimePasses imp


	/// <remarks>
	/// Spirit Actions should not call this directly but rather go through Space.Tokens => ActionScope.AccessTokens()
	/// UI or Test stuff that is outside of an ActionScope, may use this directly.
	/// </remarks>
	public Space this[SpaceSpec space] => new Space( space, GetTokensCounts( space ), _islandMods.Keys, this );

	CountDictionary<ISpaceEntity> GetTokensCounts( SpaceSpec key ) => _tokenCounts.Get( key, () => new CountDictionary<ISpaceEntity>() );

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	public int GetDynamicTokensFor( Space space, TokenClassToken token ) 
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

			// == Restore TokenCounts ==
			// update existing
			foreach (SpaceSpec spaceSpec in _tokenCounts.Keys) {
				// stasis
				spaceSpec.DoesExists = true; // when false, set below

				// Token counts
				Space space = src[spaceSpec];
				CountDictionary<ISpaceEntity> savedCounts = _tokenCounts[spaceSpec];
				// remove old types
				foreach(var oldKey in space.Keys.Except(savedCounts.Keys).ToArray())
					space.Init(oldKey,0);
				// set current types
				foreach (var (token,count) in savedCounts.Select(x=>(x.Key,x.Value))) {
					space.Init(token, count);
					if(token is ITrackMySpaces tms)
						tms.TrackAdjust(space,count);
				}
			}
			// remove old
			foreach(var remove in src._tokenCounts.Keys.Except(_tokenCounts.Keys).ToArray())
				src._tokenCounts.Remove(remove);

			foreach(var space in _doesNotExist) space.DoesExists = false;
			// Restore Defaults
			src.TokenDefaults.Clear();
			foreach(var pair in tokenDefaults)
				src.TokenDefaults.Add(pair.Key,pair.Value);
			// Restore Dynamic tokens
			src.Dynamic.Memento = _dynamicTokens;
		}
		readonly Dictionary<SpaceSpec, CountDictionary<ISpaceEntity>> _tokenCounts = [];
		readonly SpaceSpec[] _doesNotExist;
		readonly Dictionary<ITokenClass, HumanToken> tokenDefaults = [];
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

	readonly Dictionary<SpaceSpec, CountDictionary<ISpaceEntity>> _tokenCounts = [];

}
