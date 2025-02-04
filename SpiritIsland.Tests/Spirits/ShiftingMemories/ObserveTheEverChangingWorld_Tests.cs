namespace SpiritIsland.Tests.Spirits.ShiftingMemoryNS;

public class ObserveTheEverChangingWorld_Tests {

	[Fact]
	public async Task RevealedTokens_GainElements() {

		var spirit = new ShiftingMemoryOfAges();
		var gs = new SoloGameState(spirit, Boards.A);
		gs.Initialize();

		// Given: enough elements to trigger level-1 innate
		spirit.Configure().Elements("1 moon");
		//   And: no prepared water
		spirit.PreparedElementMgr[Element.Water].ShouldBe(0);

		// When: triggers Observe the Ever-changing World (SUT) (level 1 Observe)
		await spirit.InnatePowers[1].ActivateAsync(spirit).AwaitUser(user => {
			user.NextDecision.HasPrompt("Observe the Ever-Changing World: Target Space").HasOptions("A5,A7,A8").Choose("A5");
			user.NextDecision.HasPrompt("Select Innate Option").HasOptions("Use existing => 1 moon,Prepare 1 moon 1 air => 2 moon 1 air,Done").Choose("Use existing => 1 moon");
			user.NextDecision.HasPrompt("Prepare Element (Observe the Ever-Changing World)").HasOptions("Sun,Moon,Fire,Air,Water,Earth,Plant,Animal").Choose("Water");
		}).ShouldComplete();

		spirit.PreparedElementMgr.PreparedElements[Element.Water].ShouldBe(1);
	}

	[Fact]
	public void PrepairedElementsMayHaveMultipleStacksOnASpace() {
		var tokens = new CountDictionary<ISpaceEntity>();

		ShiftingMemoryOfAges spirit = new ShiftingMemoryOfAges();
		Board board = Boards.A;
		var gs = new SoloGameState( spirit, board );
		TargetSpaceCtx ctx = spirit.Target( board[5] );

		var el1 = new ObserveWorldMod( ctx );
		var el2 = new ObserveWorldMod( ctx );

		tokens[el1] = 1;
		tokens[el2] = 2;

		tokens[el1].ShouldBe( 1 );
		tokens[el2].ShouldBe( 2 );
	}

}
