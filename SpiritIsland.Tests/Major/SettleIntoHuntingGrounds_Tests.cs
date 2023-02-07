namespace SpiritIsland.Tests.Major;

public class SettleIntoHuntingGrounds_Tests {

	// -- Special Rules --
	// MovePresenceWithToken( thunderspeaker & Sharp Fangs )
	// !!! Mists Shift and Flow(Shroud special rule)

	// -- Power --
	// Draw Towards an Everconsuming void
	// Pour Time Sideways( move from 1 presence land to another )
	// Flowing and Silent Forms Dart By
	// - PushInsteadOfDestroy
	// - Gather Presence / SS of another spirit
	// Powered By the Furnace Of the Earth

	// Placing Presencee (taking from board)

	// -- Growth: Don't need tested --
	// MovePresence
	// Gather Into Ocean
	// Push From Ocean

	[Fact]
	public async Task CanFreezePresence() {
		var (spirit,gameState,board) = Init();
		var a5 = gameState.Tokens[ board[5] ];

		// Given: Presence on a5 and a6
		SpiritExtensions.Adjust( spirit.Presence, a5, 1 );

		//  When: played Settle Into Hunting Grounds
		await using var actionScope = gameState.StartAction(ActionCategory.Spirit_Power );
		Task task = SettleIntoHuntingGrounds.ActAsync( spirit.BindMyPowers(gameState) );
		task.IsCompleted.ShouldBeTrue();



	}

	static (Spirit,GameState,Board) Init() {
		var spirit = new RiverSurges();
		var board = Board.BuildBoardA();
		var gameState = new GameState(spirit,board);
		return (spirit,gameState,board);
	}

}
