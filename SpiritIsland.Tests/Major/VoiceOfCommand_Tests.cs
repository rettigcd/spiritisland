namespace SpiritIsland.Tests.Major; 


public class VoiceOfCommand_Tests {

	[Fact]
	public async Task DuringRavage_ExplorersDefend() {
		Spirit spirit = new RiverSurges();
		Board board = Board.BuildBoardA();
		_ = new GameState(spirit,board);
		var space = board[7].ScopeSpace;

		// Given: Voice of Command played on space
		await VoiceOfCommand.ActAsync( spirit.Target(space.SpaceSpec)).ShouldComplete("VoC");

		//   And: after power is done, land somehow gets 4 explorers & 2 Town
		space.InitDefault( Human.Explorer, 4 );
		space.InitDefault( Human.City, 1 );

		//  When: ravaging
		// 1 city does 3 damage - defend:2 = 1 damage, killing 1 explorers, leaving 4-1=3 explorers
		// 3 explorers do 3 damage, killing 1 city, 0
		await space.Ravage().ShouldComplete("ravage");

		//  Then: 2 explorers remain
		space.Summary.ShouldBe("3E@1,2G");
	}
}
