namespace SpiritIsland.Tests.Spirits.ShiftingMemoryNS;

public class PreparedElements {

	[Fact]
	public void TwoStacksOnASpace() {
		var tokens = new CountDictionary<ISpaceEntity>();

		ShiftingMemoryOfAges spirit = new ShiftingMemoryOfAges();
		Board board = Board.BuildBoardA();
		_ = new GameState( spirit, board );
		TargetSpaceCtx ctx = spirit.Bind().Target(board[5]);

		var el1 = new ObserveWorldMod(ctx);
		var el2 = new ObserveWorldMod(ctx);

		tokens[el1] = 1;
		tokens[el2] = 2;

		tokens[el1].ShouldBe(1);
		tokens[el2].ShouldBe(2);
	}

}

