
namespace SpiritIsland.Tests.Basegame.Spirits.BringerNS;

public class Bringer_GrowthTests : GrowthTests {

	static Spirit InitSpirit() {
		return new Bringer {
			CardDrawer = new PowerProgression(
				PowerCard.For<VeilTheNightsHunt>(),
				PowerCard.For<ReachingGrasp>()
			),
		};
	}

	public Bringer_GrowthTests():base( InitSpirit() ) {}

	[Fact] 
	public void ReclaimAll_PowerCard() { // Growth Option 1

		// reclaim, +1 power card
		Given_HalfOfPowercardsPlayed();

		_ = When_Growing( 0 );

		// Then:
		Assert_AllCardsAvailableToPlay( 4 + 1 );
		Assert_HasCardAvailable( "Veil the Night's Hunt" );

	}

	[Fact] 
	public void Reclaim1_Presence() { // Growth Option 2
		// reclaim 1, add presense range 0
		Given_HalfOfPowercardsPlayed();
		Given_HasPresence( board[4] );

		_ = When_Growing( 1 );

		User.Growth_Reclaims1("Predatory Nightmares $2 (Slow),{Dreams of the Dahan $0 (Fast)}");
		User.Growth_PlacesPresence( "energy>A4" );

		spirit.Hand.Count.ShouldBe( 3 );
	}

	[Fact] 
	public void PowerCard_Presence() { // Growth Option 3
		// +1 power card, +1 pressence range 1
		Given_HasPresence( board[1] );

		_ = When_Growing( 2 );

		User.Growth_DrawsPowerCard();
		User.Growth_PlacesEnergyPresence( "A1;A2;A4;A5;A6" );

		Assert_GainsFirstPowerProgressionCard(); // gains 1st card in power progression
		Assert_BoardPresenceIs( "A1A1" );
	}

	[Fact] 
	public void PresenseOnPieces_Energy(){ // Growth Option 4

		board = LineBoard.MakeBoard();
		gameState = new GameState( spirit, board );

		Given_HasPresence(board[5]);
		gameState.DahanOn(board[6]).Init(1);
		gameState.Tokens[board[7]].AdjustDefault(Invader.Explorer,1);
		gameState.Tokens[board[8]].AdjustDefault(Invader.Town,1);
		gameState.Tokens[board[0]].AdjustDefault(Invader.City,1);

		// add presense range 4 Dahan or Invadors, +2 energy
		When_StartingGrowth();

		//User.Growth_SelectsOption( "GainEnergy(2) / PlacePresence(4,dahan or invaders)" );
		User.Growth_SelectAction( $"PlacePresence(4,{Target.Dahan}Or{Target.Invaders})" );
		User.Growth_PlacesEnergyPresence( "T6;T7;T8;T9" );

		Assert.Equal(2,spirit.EnergyPerTurn);
		Assert_HasEnergy(2+2);
		Assert_BoardPresenceIs("T5T6");
	}

	[Trait("Presence","EnergyTrack")]
	[Theory]
	[InlineDataAttribute(1,2,"")]
	[InlineDataAttribute(2,2,"air")]
	[InlineDataAttribute(3,3,"air")]
	[InlineDataAttribute(4,3, "moon,air" )]
	[InlineDataAttribute(5,4, "moon,air" )]
	[InlineDataAttribute(6,4, "moon,air,any" )]
	[InlineDataAttribute(7,5, "moon,air,any" )]
	public async Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		var fixture = new ConfigurableTestFixture { Spirit = new Bringer() };
		await fixture.VerifyEnergyTrack(revealedSpaces, expectedEnergyGrowth, elements);
	}

	[Trait("Presence","CardTrack")]
	[Theory]
	[InlineDataAttribute(1,2,"")]
	[InlineDataAttribute(2,2,"")]
	[InlineDataAttribute(3,2,"")]
	[InlineDataAttribute(4,3,"")]
	[InlineDataAttribute(5,3,"")]
	[InlineDataAttribute(6,3,"any")]
	public async Task CardTrack(int revealedSpaces, int expectedCardPlayCount, string elements){
		var fixture = new ConfigurableTestFixture { Spirit = new Bringer() };
		await fixture.VerifyCardTrack(revealedSpaces, expectedCardPlayCount, elements);
	}

	void Assert_GainsFirstPowerProgressionCard() {
		Assert_HasCardAvailable( "Veil the Night's Hunt" );
	}

}
