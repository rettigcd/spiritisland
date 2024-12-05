namespace SpiritIsland.Tests.Major;

public class DreamOfTheUntouchedLand_Tests {

	[Fact]
	public async Task AddBoard() {
		var gs = new SoloGameState();

		int layoutChangedCount = 0;
		gs.NewLogEntry += obj => { if(obj is Log.LayoutChanged) layoutChangedCount++; };

		// Given: spirit has threshold elements
		gs.Spirit.Configure().Elements("3 moon,2 water,3 earth,2 plant");

		/// When: spirit plays card
		await DreamOfTheUntouchedLand.ActAsync(gs.Spirit.Target(gs.Board[1])).AwaitUser(u => {
			u.NextDecision.HasPrompt("Activate Element Threshold?").Choose("Yes");
			u.NextDecision.HasPrompt("Add Beast to:").HasOptions("F1,F2,F3,F4,F5,F6,F7,F8").Choose("F1");
			u.NextDecision.HasPrompt("Add Beast to:").HasOptions("F1,F2,F3,F4,F5,F6,F7,F8").Choose("F2");
			u.NextDecision.HasPrompt("Add Wilds to:").HasOptions("F1,F2,F3,F4,F5,F6,F7,F8").Choose("F3");
			u.NextDecision.HasPrompt("Add Wilds to:").HasOptions("F1,F2,F3,F4,F5,F6,F7,F8").Choose("F4");
			u.NextDecision.HasPrompt("Add Badlands to:").HasOptions("F1,F2,F3,F4,F5,F6,F7,F8").Choose("F5");
			u.NextDecision.HasPrompt("Add Badlands to:").HasOptions("F1,F2,F3,F4,F5,F6,F7,F8").Choose("F6");
			u.NextDecision.HasPrompt("Select Presence to place").ChooseFirst();
			u.NextDecision.HasPrompt("Where would you like to place your presence?").HasOptions("F1,F2,F3,F4,F5,F6,F7,F8").Choose("F5");
			u.NextDecision.HasPrompt("Select Presence to place").ChooseFirst();
			u.NextDecision.HasPrompt("Where would you like to place your presence?").HasOptions("F1,F2,F3,F4,F5,F6,F7,F8").Choose("F6");
		});

		// Then: 2 boards
		gs.Island.Boards.Length.ShouldBe(2);

		//  And: we got 1 layout changed event
		layoutChangedCount.ShouldBe(1);

		//  And: board A is attached to F
		Neighbors(gs.Board[3]).ShouldBe("A0,A2,A4,F4,F7");
		Neighbors(gs.Board[4]).ShouldBe("A1,A2,A3,A5,F3,F4");
		Neighbors(gs.Board[5]).ShouldBe("A1,A4,A6,A7,A8,F3");
		Neighbors(gs.Board[7]).ShouldBe("A5,A8,F3");
		string Neighbors(SpaceSpec space) => space.Adjacent_Existing.Select(x => x.Label).Order().Join(",");
	}

}