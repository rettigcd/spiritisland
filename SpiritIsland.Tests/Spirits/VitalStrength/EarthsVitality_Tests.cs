namespace SpiritIsland.Tests.Spirits.VitalStrengthNS;

[Collection("BaseGame Spirits")]
public sealed class EarthsVitality_Tests {

	static (VitalStrength spirit, SoloGameState gs) Init() {
		var spirit = new VitalStrength();
		var gs = new SoloGameState( spirit, Boards.A );
		return (spirit, gs);
	}

	[Fact]
	[Trait( "SpecialRule", EarthsVitality.Name )]
	public void StartingSacredSite_HasDefend3() {
		var (spirit, gs) = Init();
		gs.Initialize();

		// Vital Strength starts with 2 presence (a sacred site) in the highest-numbered mountain, A6
		gs.Tokens[gs.Board[6]].Defend.Count.ShouldBe( 3 );
	}

	[Fact]
	[Trait( "SpecialRule", EarthsVitality.Name )]
	public async Task Defend_ComesBackEveryRound_OnEachSacredSite() {
		var (spirit, gs) = Init();
		Space sacredSite = gs.Tokens[gs.Board[5]];
		Space single = gs.Tokens[gs.Board[6]];
		spirit.Given_IsOn( sacredSite, 2 );
		spirit.Given_IsOn( single, 1 );

		// When: a round ends (Defend is cleared when time passes)
		await gs.TriggerTimePasses();

		// Then: the sacred site has Defend 3 again, the land with 1 presence has none
		sacredSite.Defend.Count.ShouldBe( 3 );
		single.Defend.Count.ShouldBe( 0 );
	}

	[Fact]
	[Trait( "SpecialRule", EarthsVitality.Name )]
	public async Task LosingTheSacredSite_AfterARound_TakesItsDefend_AndDoesNotThrow() {
		// (used to throw "Defend sync issue with Earths Vitality": the Defend was given once, then cleared at the end of
		//  the round, so there was none left to take back)
		var (spirit, gs) = Init();
		Space sacredSite = gs.Tokens[gs.Board[5]];
		spirit.Given_IsOn( sacredSite, 2 );
		spirit.Given_IsOn( gs.Tokens[gs.Board[6]], 1 ); // elsewhere too: losing the last presence would end the game
		await gs.TriggerTimePasses();
		sacredSite.Defend.Count.ShouldBe( 3 );

		// When: 1 presence is destroyed there, so it is no longer a sacred site
		await sacredSite.Destroy( spirit.Presence.Token, 1 );

		// Then: its Defend is gone
		sacredSite.Defend.Count.ShouldBe( 0 );
	}

	[Fact]
	[Trait( "SpecialRule", EarthsVitality.Name )]
	public async Task LosingTheSacredSite_WithNoDefendLeft_DoesNotThrow() {
		// Defend 3 may already be gone (cleared, or the land was never given it): take away only what is there.
		var (spirit, gs) = Init();
		Space sacredSite = gs.Tokens[gs.Board[5]];
		spirit.Given_IsOn( sacredSite, 2 );
		spirit.Given_IsOn( gs.Tokens[gs.Board[6]], 1 );
		sacredSite.Defend.Count.ShouldBe( 0 );

		await sacredSite.Destroy( spirit.Presence.Token, 1 );

		sacredSite.Defend.Count.ShouldBe( 0 );
	}
}
