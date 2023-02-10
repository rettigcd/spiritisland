using static SpiritIsland.JaggedEarth.ObserveTheEverChangingWorld;

namespace SpiritIsland.Tests.Spirits.ShiftingMemoryNS;

public class PreparedElements {

	[Fact]
	public void TwoStacksOnASpace() {
		var tokens = new CountDictionary<ISpaceEntity>();

		var spirit = new ShiftingMemoryOfAges();
		var board = Board.BuildBoardA();
		var gs = new GameState( spirit, board );
		_ = gs.StartAction( ActionCategory.Spirit_Power ); // !!! dispose or get rid of if we don't need it.
		var ctx = spirit.BindMyPowers(gs).Target(board[5]);

		var el1 = new ObserveWorldMod(ctx);
		var el2 = new ObserveWorldMod(ctx);

		tokens[el1] = 1;
		tokens[el2] = 2;

		tokens[el1].ShouldBe(1);
		tokens[el2].ShouldBe(2);
	}

}

