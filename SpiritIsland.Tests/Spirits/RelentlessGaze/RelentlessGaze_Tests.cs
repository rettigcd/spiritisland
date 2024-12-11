using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.RelentlessGaze; 
public class RelentlessGaze_Tests {

	[Trait("spirit","Relentless Gaze")]
	[Fact]
	public async Task RelentlessPunsihment_CanRepeatCards() {

		// Given: Reletless Gaze
		var spirit = new RelentlessGazeOfTheSun();
		var gs = new SoloGameState(spirit);

		//   And: Spirit has Teaming Rivers in its hand
		spirit.AddActionFactory(PowerCard.For(typeof(TeemingRivers)));

		//   And: lots of energy
		spirit.Energy=20;

		//   And: 3 presence in land 5
		gs.Board[5].ScopeSpace.Init(spirit.Presence.Token,3);

		//   And: in slow phase
		gs.Phase = Phase.Slow;

		//  When: spirit does slow phase
		await spirit.SelectAndResolveActions(gs).AwaitUser(user => {
			user.NextDecision.HasPrompt("Select Slow to resolve").HasOptions("Teeming Rivers $1 (Slow),Done").ChooseFirst();
			user.NextDecision.HasPrompt("Teeming Rivers: Target Space").HasOptions("A1,A2,A5,A6").Choose("A1");
			user.NextDecision.HasPrompt("Select Slow to resolve").HasOptions("Repeat Teeming Rivers on A1 for 2 energy.,Done").ChooseFirst();
			user.NextDecision.HasPrompt("Select Slow to resolve").HasOptions("Repeat Teeming Rivers on A1 for 3 energy.,Done").ChooseFirst();
			user.NextDecision.HasPrompt("Select Slow to resolve").HasOptions("Repeat Teeming Rivers on A1 for 4 energy.,Done").ChooseFirst();
			user.NextDecision.HasPrompt("Select Slow to resolve").HasOptions("Repeat Teeming Rivers on A1 for 5 energy.,Done").Choose("Done");
		}).ShouldComplete("Slow phase");
		   //  Then: card was played 4 times
		gs.Board[1].ScopeSpace.Summary.ShouldBe("4A");
	}

	[Trait("spirit", "Relentless Gaze")]
	[Fact]
	public async Task GainEnergyTwice() {
		var spirit = new RelentlessGazeOfTheSun();
		var gs = new SoloGameState(spirit);

		// Given spirit is on A5
		spirit.Given_IsOn(gs.Board[5]);

		const string selectGrowth = "Select Growth";
		const string pp = "Place Presence(2)";
		const string m3 = "Move up to 3 Presence together";
		const string x2Energy = "Gain Energy an additional time";
		const string notUsed = "Reclaim All,Add up to 3 Destroyed Presence - Range 1,Gain Power Card";

		// When Spirit grows
		await spirit.When_Growing(user => {
			// And Places Presence
			user.NextDecision.HasPrompt(selectGrowth).HasOptions(pp+","+notUsed+","+x2Energy+","+m3).Choose(pp);
			// Revealing the 2-energy slot
			user.NextDecision.HasPrompt("Select Presence to place").Choose("2,sun energy");
			user.NextDecision.Choose("A5");
			// And Chooses x2 energy
			user.NextDecision.HasPrompt(selectGrowth).HasOptions(notUsed+","+x2Energy+","+m3).Choose(x2Energy);
			// And finishes out growth (we don't care about this)
			user.NextDecision.HasPrompt(selectGrowth).HasOptions(m3).Choose(m3);
			user.NextDecision.HasPrompt("Move up to (2)").Choose("Done");
		});

		// Then: they have twice as much energy
		spirit.Energy.ShouldBe(2*2);
	}

	[Trait("spirit", "Relentless Gaze")]
	[Fact]
	public async Task GainNormalEnergy() {
		var spirit = new RelentlessGazeOfTheSun();
		var gs = new SoloGameState(spirit);

		// Given spirit is on A5
		spirit.Given_IsOn(gs.Board[5]);

		const string selectGrowth = "Select Growth";
		const string pp = "Place Presence(2)";
		const string reclaim = "Reclaim All";
		const string addDestroyed = "Add up to 3 Destroyed Presence - Range 1";
		const string notUsed = "Gain Power Card,Gain Energy an additional time,Move up to 3 Presence together";

		// When Spirit grows
		await spirit.When_Growing(user => {
			// And Places Presence
			user.NextDecision.HasPrompt(selectGrowth).HasOptions(pp + "," + reclaim + "," + addDestroyed + "," + notUsed).Choose(pp);
			// Revealing the 2-energy slot
			user.NextDecision.HasPrompt("Select Presence to place").Choose("2,sun energy");
			user.NextDecision.Choose("A5");

			// And chooses place up to 3 destroyed presence
			user.NextDecision.HasPrompt(selectGrowth).HasOptions(reclaim + "," + addDestroyed + "," + notUsed).Choose(addDestroyed);
			// And we get "Reclaim" for free...
		});

		// Then: they have normal energy (that matches Energy track of 2,sun)
		spirit.Energy.ShouldBe(2);
	}


}
