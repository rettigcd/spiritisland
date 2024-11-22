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

	CountDictionary<ISpaceEntity> GetTokensCounts( SpaceSpec key ) => _tokenCounts.Get( key, () => [] );

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
		public void Restore( Tokens_ForIsland dstSpaces ) {

			// !!! BUG - If we are going to clear Trackable, then we need to Counts also

			// Clear out the ITrackMySpaces so that .Adjust below will properly initialize
/*			var trackable = dstSpaces._tokenCounts.Values
				.SelectMany(countDict=>countDict.Keys.OfType<ITrackMySpaces>())
				.Distinct()
				.ToArray();
			foreach(ITrackMySpaces t in trackable) t.Clear();
*/

			// == Restore TokenCounts ==
			// 1st do all removal (so tokens that can only be in 1 place at a time don't get mad)
			foreach (SpaceSpec spaceSpec in _tokenCounts.Keys) {
				// stasis
				spaceSpec.DoesExists = true; // when false, set below

				// Token counts
				Space dstSpace = dstSpaces[spaceSpec];

				// remove types we no longer need
				foreach( ISpaceEntity oldKey in dstSpace.Keys.Except(_tokenCounts[spaceSpec].Keys).ToArray())
					dstSpace.Init(oldKey,0);
			}

			// 2nd do all Add (so tokens that can only be in 1 place at a time don't get mad)
			foreach( SpaceSpec spaceSpec in _tokenCounts.Keys ) {

				// Token counts
				Space dstSpace = dstSpaces[spaceSpec];

				// set types we are restoring from backup.
				foreach( var (token, count) in _tokenCounts[spaceSpec].Select(x => (x.Key, x.Value)) )
					dstSpace.Init(token, count);

			}

			// For spaces that were added (like merged Multi-Space), remove the entire Space from the dictionary
			foreach( SpaceSpec remove in dstSpaces._tokenCounts.Keys.Except(_tokenCounts.Keys).ToArray())
				dstSpaces._tokenCounts.Remove(remove);

			foreach(var space in _doesNotExist) space.DoesExists = false;

			// Restore Defaults
			dstSpaces.TokenDefaults.Clear();
			foreach(var pair in tokenDefaults)
				dstSpaces.TokenDefaults.Add(pair.Key,pair.Value);
			// Restore Dynamic tokens
			dstSpaces.Dynamic.Memento = _dynamicTokens;
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
