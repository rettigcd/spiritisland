using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Blight; 
public class SlowDissolutionOfWill_Tests {

	[Fact]
	public async Task RemovedPresence_RemovesBackgroundTracking() {
		var gs = new SoloGameState(new RiverSurges(), Boards.B);
		Spirit spirit = gs.Spirit;
		Board board = gs.Board;

		// Given: Spirit has presence on a7 and a8
		board[7].ScopeSpace.Init(spirit.Presence.Token,1);
		board[8].ScopeSpace.Init(spirit.Presence.Token,1);

		// Given: Slow Dissolution of will played
		await new SlowDissolutionOfWill().Immediately.ActAsync(gs)
			//   And: spirit has chosen replacement token
			.AwaitUser(user=>user.NextDecision
			.HasPrompt("Choose Badlands, Beast, or Wilds as Spirit-Replacement token")
			.HasOptions("Badlands,Beast,Wilds")
			.Choose("Wilds"));

		//  When: start next invader phase
		await gs.RunPreInvaderActions()
		//   and: a presence is removed
			.AwaitUser(user=>{ 
				user.NextDecision.HasPrompt("Replace Presence with Wilds")
					.HasOptions("RSiS on B7,RSiS on B8")
					.Choose("RSiS on B8");
			});

		//  Then: space does not appear in any BG-tracking properties
		spirit.Presence.Lands.Select(x=>x.SpaceSpec.Label).Join(",").ShouldBe("B7");
		spirit.Presence.IsOn( board[8].ScopeSpace ).ShouldBeFalse();
		spirit.Presence.CountOn( board[8].ScopeSpace ).ShouldBe(0);
		board[8].ScopeSpace.Has(spirit.Presence).ShouldBeFalse();
		spirit.Presence.Deployed
			.Select(d=>d.ToString() )
			.Join(",")
			.ShouldBe("RSiS on B7");

	}

}
