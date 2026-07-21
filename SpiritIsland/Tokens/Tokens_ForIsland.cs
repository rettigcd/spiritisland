namespace SpiritIsland;

public sealed class Tokens_ForIsland : IRunWhenTimePasses, IHaveMemento {

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

	// Decrements rather than zeroing - CountDictionary's indexer already removes the key once it
	// hits 0, and the inverse of AddIslandMod's ++ is --, not "reset to whatever count happened to
	// be there." No current caller adds the same instance more than once, but this stays correct if
	// one ever does.
	public void RemoveIslandMod( BaseModEntity token ) { --_islandMods[token]; }

	#region IRunWhenTimePasses imp

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	Task IRunWhenTimePasses.TimePasses( GameState gameState ) {
		foreach(var pair in _tokenCounts)
			new Space( pair.Key, pair.Value, _islandMods.Keys ).TimePasses();
		return Task.CompletedTask;
	}

	#endregion IRunWhenTimePasses imp


	/// <remarks>
	/// Spirit Actions should not call this directly but rather go through Space.Tokens => ActionScope.AccessTokens()
	/// UI or Test stuff that is outside of an ActionScope, may use this directly.
	/// </remarks>
	public Space this[SpaceSpec space] => new Space( space, GetTokensCounts( space ), _islandMods.Keys );

	CountDictionary<ISpaceEntity> GetTokensCounts( SpaceSpec key ) => _tokenCounts.Get( key, () => [] );

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	#region Memento

	object IHaveMemento.Memento {
		get => new MyMemento( this );
		set {
			((MyMemento)value).Restore( this );
		}
	}

	class MyMemento {
		public MyMemento(Tokens_ForIsland src) {
			// Save TokenCounts
			foreach(var (space,countsDict) in src._tokenCounts.Select( x => (x.Key, x.Value) ))
				_tokenCounts[space] = countsDict.Clone();
			// Save Defaults
			tokenDefaults = src.TokenDefaults.ToDictionary(p=>p.Key,p=>p.Value);
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
		}
		readonly Dictionary<SpaceSpec, CountDictionary<ISpaceEntity>> _tokenCounts = [];
		readonly SpaceSpec[] _doesNotExist;
		readonly Dictionary<ITokenClass, HumanToken> tokenDefaults = [];
	}

	#endregion Memento

	#region Json

	/// <summary>
	/// Named keys - `TokenDefaults` is `{ tokenClassLabel: humanTokenKey, ... }`; `Spaces` is
	/// `{ spaceLabel: { DoesExist, Tokens: { entityKey: count, ... } }, ... }` - every entity is just its
	/// bare lookup-table key now, never inline detail (see ISerializationContext's remarks on
	/// InternSpaceEntity), and each space's own token counts are a plain key-&gt;count object rather than
	/// an array of pairs, since each space's entities are already unique keys (a CountDictionary); same
	/// reasoning is why `Spaces` itself is keyed by label rather than an array of entries - space labels
	/// are already unique. The "Island-Mods" entry round-trips through the same shape as any other space
	/// - see SpaceSpecOrFakeByLabel for how its FakeSpace key resolves back to the one this class's own
	/// constructor already created, rather than needing a separate registry.
	///
	/// `EntityLookup` is computed last - `TokenDefaults`/`Spaces` above are built into local variables
	/// first (where entities actually get interned, both directly and via nested ISerializableSpaceEntity
	/// mods like ToDreamAThousandDeaths/BlisteringHeat/UnnervingPall), so by the time `ctx.LookupTableToJson()`
	/// runs, every entity referenced anywhere in this section has already been interned. This covers every
	/// HumanToken/ITokenClass/ISerializableSpaceEntity reference in the whole engine that flows through a
	/// Space's token dictionary - nothing outside Tokens_ForIsland's own subtree does - so scoping the
	/// table here (rather than at GameState) keeps it self-contained: tests that round-trip
	/// Tokens_ForIsland in isolation (not through a full GameState.ToJson/RestoreFromJson) still work.
	/// </summary>
	public JsonObject ToJson( ISerializationContext ctx ) {
		var defaults = new JsonObject();
		foreach( var pair in TokenDefaults )
			defaults[pair.Key.Label] = ctx.SerializeHumanToken( pair.Value );

		var spaces = new JsonObject();
		foreach( var pair in _tokenCounts )
			spaces[pair.Key.Label] = new JsonObject {
				["DoesExist"] = pair.Key.DoesExists,
				["Tokens"] = SerializeCounts( pair.Value, ctx )
			};

		return new JsonObject {
			["TokenDefaults"] = defaults,
			["Spaces"] = spaces,
			["EntityLookup"] = ctx.LookupTableToJson()
		};
	}

	static JsonObject SerializeCounts( CountDictionary<ISpaceEntity> counts, ISerializationContext ctx ) {
		var obj = new JsonObject();
		foreach( var pair in counts )
			obj[ctx.InternSpaceEntity( pair.Key )] = pair.Value;
		return obj;
	}

	/// <summary>
	/// Restores onto an existing `target` rather than building a fresh instance - clears every existing
	/// per-space `CountDictionary`'s *contents* in place (including `_islandMods`) rather than replacing
	/// the dictionary objects themselves (previously done via `target._tokenCounts.Clear()`, which
	/// discards the Space->CountDictionary mapping entirely and forces `GetTokensCounts` to fabricate
	/// brand-new CountDictionary instances below): a `Space` built earlier via `Tokens_ForIsland`'s
	/// indexer (e.g. a UI SpaceModel, which captures one permanently at construction) holds a `readonly`
	/// reference straight to one of these dictionaries, so swapping the dictionary out from under it
	/// would silently and permanently disconnect it from live data on every future restore - the exact
	/// bug this once was (board token displays freezing at whatever they showed just before the first
	/// rewind, since only rewinds ever call this). Clearing in place means every existing reference
	/// keeps observing the same objects, correctly emptied. Whatever tokens/mods were on `target` before
	/// this call (e.g. an Adversary's `Init`/`Adjust` mutating a normally-constructed board before
	/// restore - see docs/GameSerialization-Roadmap.md section 9) are discarded rather than
	/// double-applied; JSON is the sole source of truth for everything `_tokenCounts` covers afterward.
	/// Static (not an instance method) so the target reads as an explicit parameter rather than an
	/// implicit `this` - same shape callers already reason about for "restore JSON onto a specific
	/// object."
	/// Known caveat, not addressed here: `SpaceSpec.DoesExists` lives on the shared live `SpaceSpec`
	/// object, not inside `_tokenCounts` - if something mutated it before this call for a space this JSON
	/// doesn't mention at all, that mutation isn't undone by the wipe.
	/// </summary>
	public static void FromJson( Tokens_ForIsland target, JsonObject json, ISerializationContext ctx ) {
		// Must run first - populates the table every DeserializeHumanToken/DeserializeSpaceEntity call
		// below (directly, and via nested ISerializableSpaceEntity mods) needs to resolve an entity from
		// its short key.
		ctx.LoadLookupTable( (JsonObject)json["EntityLookup"]! );

		// _islandMods is itself one of these values (inserted under "Island-Mods" in the constructor),
		// so this clears it too - no separate _islandMods.Clear() needed.
		foreach( var counts in target._tokenCounts.Values )
			counts.Clear();

		foreach( var (label, node) in (JsonObject)json["TokenDefaults"]! ) {
			var tokenClass = (HumanTokenClass)ctx.TokenClassByLabel( label );
			target.TokenDefaults[tokenClass] = ctx.DeserializeHumanToken( node! );
		}

		foreach( var (label, node) in (JsonObject)json["Spaces"]! ) {
			var spaceEntry = (JsonObject)node!;
			SpaceSpec spec = ctx.SpaceSpecOrFakeByLabel( label );
			spec.DoesExists = spaceEntry["DoesExist"]!.GetValue<bool>();

			CountDictionary<ISpaceEntity> counts = target.GetTokensCounts( spec );
			foreach( var (key, countNode) in (JsonObject)spaceEntry["Tokens"]! )
				counts[ctx.ResolveSpaceEntity( key )] = countNode!.GetValue<int>();
		}
	}

	#endregion Json

	public override string ToString() => _tokenCounts
		.Select(p=>p.Key+":"+p.Value.TokenSummary())
		.Join(" ");

	public string ToVerboseString() => _tokenCounts
		.OrderBy( p => p.Key.Label )
		.Select( p => p.Key + ":" + p.Value.TokensVerbose() )
		.Join( "\r\n" );

	readonly Dictionary<SpaceSpec, CountDictionary<ISpaceEntity>> _tokenCounts = [];

}
