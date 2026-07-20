using System.Reflection;

namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Spirit.Mods - Medium tier (docs/ISpiritMod-Types.md): mods reconstructed deterministically like Low
/// tier, but carrying small extra state (a bool/int) that needs its own ISerializableSpiritMod hook.
///
/// Several of these mod classes are internal to their own project with private state fields - by
/// design, not an oversight (they're implementation details of an aspect, not part of the engine's
/// public surface). Reflection reads/writes those exact fields here so the test exercises the real
/// state rather than a proxy, without loosening production accessibility just for testability.
/// </summary>
public class SpiritMods_MediumTier_Tests {

	// Local builder (see SpiritMods_LowTier_Tests) so NatureIncarnate/Horizons don't leak into the
	// shared TestGames.GameBuilder's Fear/Blight card pool.
	static readonly GameBuilder _builder = new(
		new SpiritIsland.Basegame.GameComponentProvider(),
		new SpiritIsland.BranchAndClaw.GameComponentProvider(),
		new SpiritIsland.FeatherAndFlame.GameComponentProvider(),
		new SpiritIsland.JaggedEarth.GameComponentProvider(),
		new SpiritIsland.NatureIncarnate.GameComponentProvider(),
		new SpiritIsland.Horizons.GameComponentProvider()
	);

	static Spirit BuildAndRestore( string spiritName, AspectConfigKey[] aspects, Action<GameState, Spirit> mutate ) {
		var cfg = new GameConfiguration().ConfigSpirits( spiritName ).ConfigBoards( "A" ).ConfigAspects( aspects );

		GameState originalGs = _builder.BuildGame( cfg );
		mutate( originalGs, originalGs.Spirits[0] );

		var originalCtx = new GameStateSerializationContext( originalGs );
		JsonObject json = originalGs.ToJson( originalCtx );

		GameState restoredGs = _builder.BuildGame( cfg );
		var ctx = new GameStateSerializationContext( restoredGs );
		restoredGs.RestoreFromJson( json, ctx );

		return restoredGs.Spirits[0];
	}

	// Private fields declared on a base class aren't found via a plain GetField on the runtime type
	// (BindingFlags.FlattenHierarchy doesn't reach private base members) - walk up the hierarchy.
	static FieldInfo FindField( Type type, string fieldName ) {
		for( Type? t = type; t is not null; t = t.BaseType ) {
			FieldInfo? field = t.GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );
			if( field is not null ) return field;
		}
		throw new InvalidOperationException( $"Field '{fieldName}' not found on {type.Name} or any base type" );
	}

	static void SetField( object obj, string fieldName, object value ) => FindField( obj.GetType(), fieldName ).SetValue( obj, value );

	static T GetField<T>( object obj, string fieldName ) => (T)FindField( obj.GetType(), fieldName ).GetValue( obj )!;

	static object CreateInternal( string fullTypeName, Assembly assembly, params object[] args ) {
		Type type = assembly.GetType( fullTypeName ) ?? throw new InvalidOperationException( $"Type '{fullTypeName}' not found" );
		ConstructorInfo ctor = type.GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )
			.First( c => c.GetParameters().Length == args.Length );
		return ctor.Invoke( args );
	}

	[Fact]
	public void FrightfulShadowsEludeDestruction_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.Basegame.Shadows.Name, [SpiritIsland.Basegame.DarkFire.ConfigKey], ( gs, s ) => {
			SetField( s.Presence.Token, "UsedThisRound", true );
		} );

		var token = spirit.Presence.Token;
		GetField<bool>( token, "UsedThisRound" ).ShouldBeTrue();
		// Same instance as spirit.Mods holds - not a second, disconnected token.
		spirit.Mods.OfType<SpiritIsland.Basegame.FrightfulShadowsEludeDestruction>().Single().ShouldBe( token );
	}

	[Fact]
	public void ReachThroughEphemeralDistance_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.Basegame.Shadows.Name, [SpiritIsland.Basegame.Reach.ConfigKey], ( gs, s ) => {
			SetField( s.PowerRangeCalc, "_usedThisRound", true );
		} );

		GetField<bool>( spirit.PowerRangeCalc, "_usedThisRound" ).ShouldBeTrue();
	}

	[Fact]
	public void SwiftnessOfLightning_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.Basegame.LightningsSwiftStrike.Name, [], ( gs, s ) => {
			var mod = s.Mods.OfType<RunSlowCardsAsFastMod_EveryRound>().Single();
			SetField( mod, "_usedCount", 3 );
		} );

		var restoredMod = spirit.Mods.OfType<RunSlowCardsAsFastMod_EveryRound>().Single();
		GetField<int>( restoredMod, "_usedCount" ).ShouldBe( 3 );
	}

	[Fact]
	public void GatherPowerFromTheCoolAndDark_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.JaggedEarth.ShroudOfSilentMist.Name, [], ( gs, s ) => {
			SetField( s.Draw, "_usedThisRound", true );
		} );

		GetField<bool>( spirit.Draw, "_usedThisRound" ).ShouldBeTrue();
	}

	[Fact]
	public void Run1SlowNonMajorAsFast_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.Basegame.LightningsSwiftStrike.Name, [], ( gs, s ) => {
			object mod = CreateInternal( "SpiritIsland.Basegame.Run1SlowNonMajorAsFast", typeof( SpiritIsland.Basegame.RiversDomain ).Assembly, s );
			SetField( mod, "_usedCount", 2 );
			s.Mods.Add( (ISpiritMod)mod );
		} );

		var restoredMods = spirit.Mods.OfType<RunSlowCardsAsFastMod_EveryRound>()
			.Where( m => m.GetType().Name == "Run1SlowNonMajorAsFast" ).ToArray();
		restoredMods.Length.ShouldBe( 1 );
		GetField<int>( restoredMods[0], "_usedCount" ).ShouldBe( 2 );
	}

	[Fact]
	public void Run1SlowPushOrGatherAsFast_SurvivesRestore() {
		Spirit spirit = BuildAndRestore( SpiritIsland.Basegame.LightningsSwiftStrike.Name, [], ( gs, s ) => {
			object mod = CreateInternal( "SpiritIsland.Horizons.Run1SlowPushOrGatherAsFast", typeof( SpiritIsland.Horizons.SunBrightWhirlwind ).Assembly, s );
			SetField( mod, "_usedCount", 1 );
			s.Mods.Add( (ISpiritMod)mod );
		} );

		var restoredMods = spirit.Mods.OfType<RunSlowCardsAsFastMod_EveryRound>()
			.Where( m => m.GetType().Name == "Run1SlowPushOrGatherAsFast" ).ToArray();
		restoredMods.Length.ShouldBe( 1 );
		GetField<int>( restoredMods[0], "_usedCount" ).ShouldBe( 1 );
	}

	[Fact]
	public void EnableEmpoweredAbductMod_UsedActionResolvesToSharedSingleton() {
		// EnableEmpoweredAbductMod itself is stateless and deterministically added (BreathOfDarknessDownYourSpine's
		// own constructor) - free after replay, same as Low tier. What needed fixing was EmpoweredAbduct
		// (the IActionFactory it compares spirit.UsedActions against by reference) not being serializable
		// at all, and - if it were serialized naively - resolving to a fresh instance instead of the
		// shared static singleton the comparison depends on.
		Spirit spirit = BuildAndRestore( SpiritIsland.NatureIncarnate.BreathOfDarknessDownYourSpine.Name, [], ( gs, s ) => {
			GetField<List<IActionFactory>>( s, "_usedActions" ).Add( SpiritIsland.NatureIncarnate.EmpoweredAbduct.Singleton );
		} );

		spirit.UsedActions.ShouldContain( SpiritIsland.NatureIncarnate.EmpoweredAbduct.Singleton );
	}

}
