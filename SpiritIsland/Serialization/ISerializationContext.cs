namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves the handful of reference kinds Medium-complexity `ISpaceEntity` types hold
/// (a `Spirit`, a `SpaceSpec`, a `TargetSpaceCtx`, an `ITokenClass` singleton) to/from
/// serialization-stable identifiers (an index or a label), instead of the live object.
/// </summary>
public interface ISerializationContext {

	int IndexOf( Spirit spirit );
	Spirit SpiritAt( int index );

	SpaceSpec SpaceSpecByLabel( string label );

	/// <summary>
	/// Like <see cref="SpaceSpecByLabel"/>, but for the token-container work: some space keys are
	/// `FakeSpace`s (island mods, a blight card's virtual space, an adversary's reminder-card space)
	/// that were never added to a Board and so can't be found there. `SpaceSpec` compares by `Label`
	/// (see `SpaceSpec.Equals`), so a freshly-constructed `FakeSpace` with the same label is
	/// indistinguishable from whatever other code already references that label - no registry needed.
	/// </summary>
	SpaceSpec SpaceSpecOrFakeByLabel( string label );

	Board BoardByName( string name );

	/// <summary>
	/// Resolves an "extra" InvaderSlot already sitting in the live GameState's own
	/// InvaderDeck.ActiveSlots (e.g. England's HighImmegrationSlot) by label - needed so a hook-list
	/// entry referencing the same slot (e.g. its IRunWhenTimePasses registration) resolves back to that
	/// one already-restored instance instead of building a second, disconnected copy via
	/// InvaderSlotRegistry. See docs/GameSerialization-Roadmap.md section 10.
	/// </summary>
	InvaderSlot InvaderSlotByLabel( string label );

	/// <summary>
	/// The game's own Tokens_ForIsland/Healer/BlightCard singletons - needed to resolve IRunWhenTimePasses/
	/// IRunBeforeInvaderPhase entries that are well-known per-game objects (TimePassesActionRegistry/
	/// PreInvaderPhaseActionRegistry) rather than name-resolvable/reconstructable types. BlightCard in
	/// particular is how the 7 self-registering blight cards (which add `this` as an
	/// IRunBeforeInvaderPhase entry) resolve back to the live GameState.BlightCard instead of
	/// constructing a fresh, disconnected copy via BlightCardRegistry.
	/// </summary>
	Tokens_ForIsland Tokens { get; }
	Healer Healer { get; }
	BlightCard BlightCard { get; }

	TargetSpaceCtx TargetSpace( Spirit spirit, SpaceSpec spec ) => spirit.Target( spec );

	ITokenClass TokenClassByLabel( string label ) => TokenClassRegistry.ByLabel( label );

	/// <summary>
	/// Finds a specific ISerializableSpaceEntity instance already sitting on a space (placed there by
	/// Tokens_ForIsland.FromJson, section 1) rather than constructing a fresh one - needed when the same
	/// entity also lives in one of GameState's hook action lists (e.g. TriggerAfterNoRavageOrBuild in
	/// _postInvaderPhaseActions) and a restore must resolve both references to the one live instance.
	/// Same recurring identity-resolution shape as InvaderSlotByLabel/BlightCard above - see
	/// docs/GameSerialization-Roadmap.md section 10.
	/// </summary>
	T ExistingSpaceEntity<T>( SpaceSpec spec ) where T : class, ISpaceEntity => Tokens[spec].OfType<T>().Single();

	/// <summary>
	/// Every `ISpaceEntity` that ends up in a Space's token dictionary (a `HumanToken` value, an
	/// `ITokenClass` singleton like `Token.Beast`, or a per-game `ISerializableSpaceEntity` mod instance)
	/// is registered here once and given a short, readable key - a Space's own token entry (and anywhere
	/// else that needs to reference one, e.g. `ToDreamAThousandDeaths`' damage log) then only ever stores
	/// that key, never repeating the full detail inline. The full `[kind, detail]` behind each key is
	/// written once, in `Tokens_ForIsland.ToJson`'s own lookup-table section (`LookupTableToJson`).
	///
	/// Key choice per kind - each already has a stable, readable identity to reuse, so no synthetic
	/// counter-based scheme is needed for any of them:
	/// - `HumanToken` - its own `ToString()` (e.g. `"E@1"`), the same string players already see in-game.
	///   Interned *by value* (`HumanToken` overrides `Equals`/`GetHashCode`), so every value-identical
	///   token anywhere - on the board or inside a mod's own state - shares one entry.
	/// - `ITokenClass` - its own `Label` (e.g. `"Beast"`). These are process-wide singletons, so interning
	///   is trivial: first occurrence wins, every later reference resolves to the exact same instance
	///   anyway via `TokenClassByLabel`.
	/// - `ISerializableSpaceEntity` - the tag its own `ToJson` already leads with (e.g.
	///   `"SpiritPresenceToken"`). Interned *by reference* (the default for a type that doesn't override
	///   `Equals`), since two distinct mod instances are two distinct board entities even if their fields
	///   happen to coincide.
	///
	/// Two different values that happen to produce the same base key (e.g. two different spirits' own
	/// `SpiritPresenceToken`, both tagged plain `"SpiritPresenceToken"`) are disambiguated with a
	/// `"#2"`/`"#3"` suffix.
	/// </summary>
	string InternSpaceEntity( ISpaceEntity entity );

	/// <summary> Resolves a key produced by `InternSpaceEntity` - only valid after `LoadLookupTable` has
	/// populated the table that key came from (`Tokens_ForIsland.FromJson` does this first, before any
	/// other step that might reference an entity by key). </summary>
	ISpaceEntity ResolveSpaceEntity( string key );

	/// <summary> The accumulated key -&gt; `[kind, detail]` table for every entity interned so far via
	/// `InternSpaceEntity` - call once, after everything else has been serialized (interning happens as a
	/// side effect of `SerializeSpaceEntity`/`SerializeHumanToken` calls throughout the rest of the
	/// tree), same "collect during the pass, write once at the end" shape `GameState.ToJson` uses for
	/// its own sections. </summary>
	JsonObject LookupTableToJson();

	/// <summary> Populates the table `InternSpaceEntity`/`ResolveSpaceEntity` read from. Must run before
	/// any other restore step that might reference an entity by key - i.e. first, even before the rest
	/// of `Tokens_ForIsland.FromJson`. </summary>
	void LoadLookupTable( JsonObject json );

	/// <summary> Convenience wrapper - same lookup table as `InternSpaceEntity`, just typed for the one
	/// call sites outside `Tokens_ForIsland` (e.g. `ToDreamAThousandDeaths`' damage log) already know
	/// they're holding a `HumanToken` specifically. </summary>
	string InternHumanToken( HumanToken token ) => InternSpaceEntity( token );

	HumanToken ResolveHumanToken( string key ) => (HumanToken)ResolveSpaceEntity( key );

	JsonNode SerializeHumanToken( HumanToken token ) => InternHumanToken( token );

	HumanToken DeserializeHumanToken( JsonNode json ) => ResolveHumanToken( json.GetValue<string>() );

	/// <summary> The full 7-field detail behind one HumanToken lookup-table entry - only called by
	/// `InternSpaceEntity`/`ResolveSpaceEntity`'s own implementation now, never per-occurrence. </summary>
	JsonArray SerializeHumanTokenDetails( HumanToken token ) {
		HumanToken.Props p = token.GetProps();
		return new JsonArray( p.Class.Label, p._rawFullHealth, p.Damage, p.StrifeCount, p.Attack, (int)p.RavageOrder, (int)p.RavageSide );
	}

	HumanToken DeserializeHumanTokenDetails( JsonArray json ) {
		var tokenClass = (HumanTokenClass)TokenClassByLabel( json[0]!.GetValue<string>() );
		int rawFullHealth = json[1]!.GetValue<int>();
		int damage = json[2]!.GetValue<int>();
		int strifeCount = json[3]!.GetValue<int>();
		int attack = json[4]!.GetValue<int>();
		var ravageOrder = (RavageOrder)json[5]!.GetValue<int>();
		var ravageSide = (RavageSide)json[6]!.GetValue<int>();
		return new HumanToken( tokenClass, rawFullHealth )
			.AddDamage( damage )
			.HavingStrife( strifeCount )
			.SetAttack( attack )
			.SetRavageOrder( ravageOrder )
			.SetRavageSide( ravageSide );
	}

	/// <summary>
	/// An `IToken` field can point at either a stable, process-wide `ITokenClass` singleton
	/// (e.g. `Token.Beast`) or a specific per-game `ISerializableSpaceEntity` instance
	/// (e.g. a particular `MarkedBeast`). Serialize/deserialize whichever one it actually is.
	/// Deliberately NOT routed through the InternSpaceEntity lookup table - this is for single-field
	/// references (e.g. FreezePresence's beast/badland), not per-space token-count entries, so there's
	/// no repeated-inline-detail problem to solve here.
	/// </summary>
	JsonArray SerializeToken( IToken token ) => token switch {
		ISerializableSpaceEntity entity => new JsonArray( "Entity", entity.ToJson( this ) ),
		ITokenClass tokenClass => new JsonArray( "Class", tokenClass.Label ),
		_ => throw new NotSupportedException( $"Don't know how to serialize IToken of type {token.GetType()}" )
	};

	IToken DeserializeToken( JsonArray json ) => json[0]!.GetValue<string>() switch {
		"Class" => (IToken)TokenClassByLabel( json[1]!.GetValue<string>() ),
		"Entity" => (IToken)SpaceEntitySerialization.Deserialize( (JsonArray)json[1]!, this ),
		var kind => throw new NotSupportedException( $"Unknown IToken kind '{kind}'" )
	};

	/// <summary>
	/// Broader than <see cref="SerializeToken"/>/<see cref="DeserializeToken"/>: those are IToken-typed:
	/// a Space's own token dictionary is keyed by the wider ISpaceEntity, which also includes HumanToken
	/// (a value, not a singleton and not ISerializableSpaceEntity). Used by Tokens_ForIsland's container
	/// serialization - every occurrence just goes through the InternSpaceEntity/ResolveSpaceEntity lookup
	/// table, so this is now a one-line wrapper rather than its own switch.
	/// </summary>
	JsonNode SerializeSpaceEntity( ISpaceEntity entity ) => InternSpaceEntity( entity );

	ISpaceEntity DeserializeSpaceEntity( JsonNode json ) => ResolveSpaceEntity( json.GetValue<string>() );

}
