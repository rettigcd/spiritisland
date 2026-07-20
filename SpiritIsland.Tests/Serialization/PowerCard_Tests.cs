namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// Round-trips for PowerCard/InnatePower identity - docs/GameSerialization-Roadmap.md section 4.
/// PowerCard/InnatePower are fully immutable (no Flipped-equivalent state), so these are pure identity
/// lookups, not state round-trips.
/// </summary>
public class PowerCard_Tests {

	[Fact]
	public void PowerCardRegistry_ResolvesMinorCard() {
		var provider = new SpiritIsland.Basegame.GameComponentProvider();
		PowerCard original = provider.MinorCards.First();

		JsonNode json = original.ToJson();
		PowerCard restored = PowerCardRegistry.Deserialize( json );

		restored.Title.ShouldBe( original.Title );
		restored.PowerType.ShouldBe( PowerType.Minor );
	}

	[Fact]
	public void PowerCardRegistry_ResolvesMajorCard() {
		var provider = new SpiritIsland.Basegame.GameComponentProvider();
		PowerCard original = provider.MajorCards.First();

		JsonNode json = original.ToJson();
		PowerCard restored = PowerCardRegistry.Deserialize( json );

		restored.Title.ShouldBe( original.Title );
		restored.PowerType.ShouldBe( PowerType.Major );
	}

	[Fact]
	public void PowerCardRegistry_ResolvesSpiritStartingCard() {
		var provider = new SpiritIsland.Basegame.GameComponentProvider();
		Spirit spirit = provider.MakeSpirit( SpiritIsland.Basegame.Shadows.Name )!;
		PowerCard original = spirit.Hand.First();

		JsonNode json = original.ToJson();
		PowerCard restored = PowerCardRegistry.Deserialize( json );

		restored.Title.ShouldBe( original.Title );
		restored.PowerType.ShouldBe( PowerType.Spirit );
	}

	[Fact]
	public void InnatePowerRegistry_ResolvesSpiritInnate() {
		var provider = new SpiritIsland.Basegame.GameComponentProvider();
		Spirit spirit = provider.MakeSpirit( SpiritIsland.Basegame.Shadows.Name )!;
		InnatePower original = spirit.InnatePowers.First();

		JsonArray json = original.ToJson();
		InnatePower restored = InnatePowerRegistry.Deserialize( json );

		restored.Title.ShouldBe( original.Title );
	}

	static IEnumerable<IGameComponentProvider> AllProviders => [
		new SpiritIsland.Basegame.GameComponentProvider(),
		new SpiritIsland.BranchAndClaw.GameComponentProvider(),
		new SpiritIsland.FeatherAndFlame.GameComponentProvider(),
		new SpiritIsland.Horizons.GameComponentProvider(),
		new SpiritIsland.JaggedEarth.GameComponentProvider(),
		new SpiritIsland.NatureIncarnate.GameComponentProvider(),
	];

	[Fact]
	public void PowerCardRegistry_ResolvesEveryCardAcrossEveryExpansion() {
		int checkedCount = 0;
		foreach( IGameComponentProvider provider in AllProviders ) {
			foreach( PowerCard card in provider.MinorCards.Concat( provider.MajorCards ) ) {
				PowerCardRegistry.Deserialize( card.ToJson() ).Title.ShouldBe( card.Title );
				++checkedCount;
			}
			foreach( string spiritName in provider.SpiritNames ) {
				Spirit spirit = provider.MakeSpirit( spiritName )!;
				foreach( PowerCard card in spirit.Hand ) {
					PowerCardRegistry.Deserialize( card.ToJson() ).Title.ShouldBe( card.Title );
					++checkedCount;
				}
				foreach( InnatePower innate in spirit.InnatePowers ) {
					InnatePowerRegistry.Deserialize( innate.ToJson() ).Title.ShouldBe( innate.Title );
					++checkedCount;
				}
			}
		}
		checkedCount.ShouldBeGreaterThan( 100 ); // sanity check this actually exercised a meaningful number of cards
	}

	// None of these titles appear in their base spirit's default InnatePowers - only reachable through
	// IAspect.NewInnates now that GameComponentProviderSeeding reads aspects declaratively instead of
	// constructing a throwaway spirit and calling ModSpirit.
	[Theory]
	[InlineData( typeof( SpiritIsland.Basegame.LightningTornSkiesIncitePandemonium ) )] // Lightning/Pandemonium
	[InlineData( typeof( SpiritIsland.Basegame.WaterEatsAwayTheDeepRootsOfEarth ) )]    // Ocean/Deeps
	[InlineData( typeof( SpiritIsland.Basegame.ReclaimedByTheDeeps ) )]                 // Ocean/Deeps
	[InlineData( typeof( SpiritIsland.Basegame.Spirits.RampantGreen.Aspects.UnbelievableGrowth ) )] // RampantGreen/Regrowth
	[InlineData( typeof( SpiritIsland.Basegame.ExaltationOfTheStormWind ) )]            // Lightning/Wind
	public void InnatePowerRegistry_ResolvesAspectExclusiveInnate( Type innateType ) {
		var original = InnatePower.For( innateType );

		InnatePower restored = InnatePowerRegistry.Deserialize( original.ToJson() );

		restored.Title.ShouldBe( original.Title );
	}

}
