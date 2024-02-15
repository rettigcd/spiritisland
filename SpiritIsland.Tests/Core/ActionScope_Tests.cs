namespace SpiritIsland.Tests.Core;

public class ActionScope_Tests {

	[Fact]
	public async Task Original_IsRestored() {
		ActionScope.Initialize(new GameState(new RiverSurges(),Board.BuildBoardE()));
		Guid childId; // grab later

		// Given: we know the default/original action scope
		Guid originalId = ActionScope.Current.Id;

		{
			// And: we create a child actions scope
			await using var childScope = await ActionScope.Start(ActionCategory.Default);
			childId = childScope.Id;

			// When: we cross the await boundary (causing a TaskLocal copy)
			await Task.Delay(1);


		}	// And: the childscope gets cleaned up/disposed of

		// Then: current scope should revert to original
		ActionScope.Current.Id.ShouldBe( originalId );

		//  And: not match the child scope
		ActionScope.Current.Id.ShouldNotBe( childId );

	}

}