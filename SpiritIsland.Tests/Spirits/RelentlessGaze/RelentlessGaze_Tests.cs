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
		spirit.AddActionFactory(PowerCard.ForDecorated(TeemingRivers.ActAsync));

		//   And: lots of energy
		spirit.Energy=20;

		//   And: 3 presence in land 5
		gs.Board[5].ScopeSpace.Init(spirit.Presence.Token,3);

		//   And: in slow phase
		gs.Phase = Phase.Slow;

		//  When: spirit does slow phase
		await spirit.SelectAndResolveActionsForTest(gs).AwaitUser(user => {
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

	[Trait("spirit","Relentless Gaze")]
	[Fact]
	public async Task RelentlessPunishment_NeedsAtLeast3PresenceInTheOriginLand() {
		// (the check was "0 or more", so a card could always be repeated)
		var spirit = new RelentlessGazeOfTheSun();
		var gs = new SoloGameState(spirit);
		spirit.AddActionFactory(PowerCard.ForDecorated(TeemingRivers.ActAsync));
		spirit.Energy=20;
		//  Given: only 2 presence in land 5
		gs.Board[5].ScopeSpace.Init(spirit.Presence.Token,2);
		gs.Phase = Phase.Slow;

		//  When: spirit uses the card
		await spirit.SelectAndResolveActionsForTest(gs).AwaitUser(user => {
			user.NextDecision.HasPrompt("Select Slow to resolve").HasOptions("Teeming Rivers $1 (Slow),Done").ChooseFirst();
			user.NextDecision.HasPrompt("Teeming Rivers: Target Space").Choose("A1");
			//  Then: it cannot be repeated - the next thing offered is not a repeat
		}).ShouldComplete("Slow phase");
	}

	[Trait("spirit","Relentless Gaze")]
	[Fact]
	public async Task RelentlessPunishment_CanRepeatCardsThatUseTheirOrigin() {
		// (used to throw NullReferenceException: Unbearable Gaze reads its origins from the targetting details,
		//  and a repeat is not targeted again)
		// Given: Relentless Gaze with Unbearable Gaze in its hand, lots of energy and 3 presence in land 5
		var spirit = new RelentlessGazeOfTheSun();
		var gs = new SoloGameState(spirit);
		spirit.AddActionFactory(PowerCard.ForDecorated(UnbearableGaze.ActAsync));
		spirit.Energy = 20;
		gs.Board[5].ScopeSpace.Init(spirit.Presence.Token,3);
		gs.Phase = Phase.Slow;
		//   And: explorers on the target land (A1) and on the origin land (A5)
		gs.Board[1].ScopeSpace.Given_HasTokens("1E@1");
		gs.Board[5].ScopeSpace.Given_HasTokens("3E@1");

		//  When: spirit uses the card, then repeats it
		await spirit.SelectAndResolveActionsForTest(gs).AwaitUser(user => {
			user.NextDecision.HasPrompt("Select Slow to resolve").ChooseFirst();
			user.NextDecision.HasPrompt("Unbearable Gaze: Target Space").Choose("A1");
			user.NextDecision.HasPrompt("Push (2)").ChooseFrom("E@1 on A1").ChooseTo("A2");
			user.NextDecision.HasPrompt("Push (1)").ChooseFrom("E@1 on A5").ChooseTo("A6");
			user.NextDecision.HasPrompt("Select Slow to resolve").HasOptions("Repeat Unbearable Gaze on A1 for 2 energy.,Done").ChooseFirst();
			//  Then: the repeat can push from the same origin (A5), like the first use did
			user.NextDecision.HasPrompt("Push (2)").ChooseFrom("E@1 on A5").ChooseTo("A6");
			user.NextDecision.HasPrompt("Push (1)").ChooseFrom("E@1 on A5").ChooseTo("A6");
			user.NextDecision.HasPrompt("Select Slow to resolve").Choose("Done");
		}).ShouldComplete("Slow phase");
	}

	[Trait("spirit", "Relentless Gaze")]
	[Fact]
	public async Task ScorchingConvergence_CannotMovePresenceFromTheTargetLandToItself() {
		// (used to offer "move from A5 to A5", which moves nothing, and asked for it again and again)
		var spirit = new RelentlessGazeOfTheSun();
		var gs = new SoloGameState(spirit);

		// Given: 2 sacred sites, A5 and next to it A6, and enough sun for the first tier
		foreach( Space s in spirit.Presence.Lands.ToArray() ) spirit.Given_IsOn(s,0);
		spirit.Given_IsOn(gs.Board[5],2);
		spirit.Given_IsOn(gs.Board[6],2);
		spirit.Elements.Add( ElementStrings.Parse("2 sun") );

		// When: it targets A5 (a sacred site itself, so it is one of the possible origins)
		await spirit.When_ResolvingInnate("Scorching Convergence", user => {
			user.NextDecision.HasPrompt("Scorching Convergence: Target Space").Choose("A5");
			// Then: the presence on A6 can be moved to A5 - and the presence already on A5 is neither offered a move to A5 nor counted
			user.NextDecision.HasPrompt("Move (2)").HasOptions("RGotS on A6 => A5").ChooseFirst();
			user.NextDecision.HasPrompt("Move (1)").HasOptions("RGotS on A6 => A5").ChooseFirst(); // all of it: 1 at a time
		});

		// And: all the presence from A6 moved, and the action ended
		gs.Board[5].ScopeSpace[spirit.Presence.Token].ShouldBe(4);
		gs.Board[6].ScopeSpace[spirit.Presence.Token].ShouldBe(0);
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
			user.NextDecision.ChooseFrom("2,sun energy").ChooseTo("A5");
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
			user.NextDecision.HasPrompt("Select Presence to place").ChooseFrom("2,sun energy").ChooseTo("A5");

			// And chooses place up to 3 destroyed presence
			user.NextDecision.HasPrompt(selectGrowth).HasOptions(reclaim + "," + addDestroyed + "," + notUsed).Choose(addDestroyed);
			// And we get "Reclaim" for free...
		});

		// Then: they have normal energy (that matches Energy track of 2,sun)
		spirit.Energy.ShouldBe(2);
	}

}
