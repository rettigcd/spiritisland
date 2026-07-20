namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for TargetingSourceStrategyRegistry/RangeCalcRegistry - docs/GameSerialization-Roadmap.md
/// section 2's "known gap" (an active non-default TargetingSourceStrategy/PowerRangeCalc used to throw).
/// Spirit_Tests covers the single-level case at the Spirit.ToJson/RestoreFromJson level; these cover the
/// two shapes that need more than one spirit or more than one recursion level.
/// </summary>
public class TargetingSourceStrategyAndRangeCalc_Tests {

	static GameStateSerializationContext CtxFor( GameState gs ) => new( gs );

	[Fact]
	public void EntwinedPresenceSource_RoundTrips_MultiSpiritDictionary() {
		var board = Boards.A;
		var spiritA = new TestSpirit();
		var spiritB = new TestSpirit();
		var gs = new GameState( [ spiritA, spiritB ], [ board ], 0 );
		var ctx = CtxFor( gs );

		Type type = typeof( SpiritIsland.Basegame.EntwinedPower ).Assembly.GetType( "SpiritIsland.Basegame.EntwinedPower+EntwinedPresenceSource" )!;
		// The public ctor is `params Spirit[] spirits` - box the Spirit[] itself as the single arg
		// object[]{ spiritArray }, not object[]{ spiritA, spiritB } (array covariance would otherwise
		// swallow a bare Spirit[] as the args array itself rather than args[0] - same trap as
		// HookActionList_Tests's PourTimeSideways case).
		object[] ctorArgs = [ new Spirit[] { spiritA, spiritB } ];
		var original = (ITargetingSourceStrategy)Activator.CreateInstance(
			type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, ctorArgs, null
		)!;

		// The constructor also assigns spirit.TargetingSourceStrategy = this for both spirits -
		// mirrors what really happens when Entwined Power is played.
		spiritA.TargetingSourceStrategy.ShouldBeSameAs( original );
		spiritB.TargetingSourceStrategy.ShouldBeSameAs( original );

		JsonArray json = original.ToJson( ctx );
		var restored = TargetingSourceStrategyRegistry.Deserialize( json, ctx );

		restored.ShouldBeOfType( type );
		// Both spirits' pre-entwined strategy (Default) round-trips through the dictionary.
		var oldsField = type.GetField( "_olds", BindingFlags.NonPublic | BindingFlags.Instance )!;
		var olds = (Dictionary<Spirit, ITargetingSourceStrategy>)oldsField.GetValue( restored )!;
		olds.Keys.ShouldBe( [ spiritA, spiritB ], ignoreOrder: true );
		olds.Values.ShouldAllBe( v => v is DefaultPowerSourceStrategy );
	}

	[Fact]
	public void RangeCalcRegistry_ResolvesNestedDecoratorChain() {
		// RangeExtender wrapping ExtendRange1FromMountain wrapping Default - proves the recursive
		// Previous!.ToJson(ctx)/RangeCalcRegistry.Deserialize(...) resolution works more than one level
		// deep, not just for a single decorator directly wrapping Default.
		var gs = new SoloGameState();
		var ctx = CtxFor( gs );

		Type mountainType = typeof( SpiritIsland.JaggedEarth.AbsoluteStasis ).Assembly.GetType( "SpiritIsland.JaggedEarth.ExaltationOfMoltenStone+ExtendRange1FromMountain" )!;
		var mountainCalc = (ICalcRange)Activator.CreateInstance(
			mountainType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, [ DefaultRangeCalculator.Singleton ], null
		)!;
		var original = new RangeExtender( 3, mountainCalc );

		JsonArray json = original.ToJson( ctx );
		var restored = RangeCalcRegistry.Deserialize( json, ctx );

		var restoredExtender = restored.ShouldBeOfType<RangeExtender>();
		restoredExtender.Previous.ShouldBeOfType( mountainType );
		restoredExtender.Previous!.Previous.ShouldBeSameAs( DefaultRangeCalculator.Singleton );
	}

}
