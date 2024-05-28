namespace SpiritIsland.Tests.Spirits.ShiftingMemoryNS;

public class ShiftingMemory_Tests {

	[Fact]
	public async Task ForgettingCardAtEndOfTurn_DiscardsItInstead() {
		var spirit = new ShiftingMemoryOfAges();
		var board = Boards.D;
		var gs = new GameState( spirit, board ) {
			MajorCards = new PowerCardDeck(
				new List<PowerCard> { 
					// instead of Majors, using cards I know well...
					PowerCard.For( typeof(FlashFloods) ),
					PowerCard.For( typeof(WashAway) ),
				},
				1, PowerType.Major
			)
		};
		gs.Initialize();

		// Given: Shiftin Memories has ample Energy
		spirit.Energy = 20;
		//   And: has all the elements
		spirit.Elements.Init(ElementStrings.Parse( "2 sun,2 moon,2 fire,2 air,2 water,2 earth,2 plant,2 animal" ) );

		//   And: Shifting Memory Resolves Unlock The Gates...
		await spirit.When_ResolvingCard<UnlockTheGatesOfDeepestPower>( u => {
			//  And: Selects Major Card
			u.NextDecision.HasPrompt( "Select spirit Power Card" ).HasOptions( "Wash Away $1 (Slow),Flash Floods $2 (Fast)" ).Choose( "Flash Floods $2 (Fast)" );;
			//  And: Chooses to Play It by Forgetting it
			u.NextDecision.HasPrompt( "Select action" ).HasOptions( "Play [Flash Floods] by paying 1,Play [Flash Floods] by forgetting it at end of turn,No Action" ).Choose( "Play [Flash Floods] by forgetting it at end of turn" ); ;
		} ).ShouldComplete();
		spirit.InPlay.Select(c=>c.Title).Join().ShouldBe("Flash Floods");
	
		// When: Time Passes
		await gs.TriggerTimePasses();

		// Then: Card is in Discard Hand( aka, not forgotten )
		spirit.DiscardPile.Select(c=>c.Title).Join().ShouldBe("Flash Floods");
	}

}