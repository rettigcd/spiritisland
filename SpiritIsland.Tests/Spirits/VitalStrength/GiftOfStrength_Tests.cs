namespace SpiritIsland.Tests.Spirits.VitalStrengthNS;

[Collection("BaseGame Spirits")]
public sealed class GiftOfStrength_Tests {

	VitalStrength spirit;

	[Fact]
	[Trait("something","repeat card")]
	public async Task Replaying_FastCards() {

		spirit = new VitalStrength();
		var gs = new SoloGameState( spirit, Boards.A );

		// Given: Spirit has lots of energy
		spirit.Energy = 20;
		//   And: Phase is Fast
		gs.Phase = Phase.Fast;
		//   And: Spirit has already resolved fast w/ cost=1
		var fast1 = PowerCard.ForDecorated( Fast1 );
		spirit.MarkAsResolved(fast1);
		//   And: resolved fast w/ cost=2
		var fast2 = PowerCard.ForDecorated(Fast2);
		spirit.MarkAsResolved(fast2);

		//  When: Spirit resolves GoS on Self
		await GiftOfStrength.Option1( spirit.Target(spirit) );

		//   And: chooses card to
		await spirit.SelectAndResolveActions(gs).AwaitUser(user => {
			user.NextDecision.HasPrompt("Select Fast to resolve").HasOptions("Replay Card (max cost:1),Done").Choose("Replay Card (max cost:1)");
			//  Then: Spirit can replay ONLY the Fast-1 card.
			user.NextDecision.HasPrompt("Select card to repeat").HasOptions("Fast-1 $1 (Fast),Done").Choose("Fast-1 $1 (Fast)");

			// cleanup...
			user.NextDecision.HasPrompt("Select Fast to resolve").HasOptions("Fast-1 $1 (Fast),Done").Choose("Done");
		}).ShouldComplete();
	}

	void PlayCards( PowerCard[] cards ) {
		foreach(var card in cards)
			spirit.PlayCard( card );
	}

	[Fact]
	[Trait( "something", "repeat card" )]
	public async Task Replaying_SlowCards() {

		spirit = new VitalStrength();
		var gs = new SoloGameState(spirit,Boards.A);

		// Given: spirit has enough elements to trigger GoS
		spirit.Elements.Add(ElementStrings.Parse("1 sun,2 earth,2 plant"));
		spirit.Energy = 20;
		//  And: Earth has 4 cards
		PowerCard[] cards = [
			PowerCard.ForDecorated( Fast0 ),
			PowerCard.ForDecorated( Slow0 ), 
			PowerCard.ForDecorated( Slow1 ), 
			PowerCard.ForDecorated( Slow2 )  
		];
		spirit.TempCardPlayBoost = cards.Length;
		spirit.Hand.AddRange(cards);
		PlayCards( cards ); // PLAY, not RESOLVE...

		//   And: phase is fast
		gs.Phase = Phase.Fast;
		//   And: resolves 1 FAST
		await spirit.ResolveActionAsync( cards[0], Phase.Fast );

		//  When: spirit resolves GoS on self  (during FAST)
		await InnatePower.For(typeof(GiftOfStrength)).ActivateAsync(spirit);

		//   And: phase is slow
		gs.Phase = Phase.Slow;
		
		await spirit.SelectAndResolveActions(gs).AwaitUser(user => {

			const string slow1 = "Slow-1 $1 (Slow)";

			//   And: resolves SLOW-1 card
			user.NextDecision.HasPrompt("Select Slow to resolve")
				.HasOptions("Slow-0 $0 (Slow),Slow-1 $1 (Slow),Slow-2 $2 (Slow),Replay Card (max cost:1),Done")
				.Choose(slow1);

			// It is no longer available, but user can select to repeat it
			user.NextDecision.HasPrompt("Select Slow to resolve")
				.HasOptions("Slow-0 $0 (Slow),Slow-2 $2 (Slow),Replay Card (max cost:1),Done")
				.Choose("Replay Card (max cost:1)");
			user.NextDecision.HasPrompt( "Select card to repeat" ).HasOptions( "Slow-1 $1 (Slow),Done" )
				.Choose(slow1);

			//  Then: the resolved slow is available to do again.
			user.NextDecision.HasPrompt("Select Slow to resolve").HasOptions("Slow-0 $0 (Slow),Slow-2 $2 (Slow),Slow-1 $1 (Slow),Done").Choose(slow1);

			user.NextDecision.HasPrompt("Select Slow to resolve").Choose("Done");
		}).ShouldComplete("Resolve Slow Actions");

	}

	// Replay-Action works in slow
	// In Slow, Replacy finds only Played-Slow
	//  - ignores fast
	//  - ignores unplayed-slow

	[SpiritCard("Slow-0",0),Slow]
	[FromPresence(Filter.Ocean,0)] // will skip the Target-Space step
	[Instructions(""),Artist("")]
	static Task Slow0(TargetSpaceCtx _) => Task.CompletedTask;

	[SpiritCard("Slow-1",1),Slow]
	[FromPresence( Filter.Ocean, 0)] // will skip the Target-Space step
	[Instructions(""), Artist("")]
	static Task Slow1(TargetSpaceCtx _) => Task.CompletedTask;

	[SpiritCard("Slow-2",2),Slow]
	[FromPresence( Filter.Ocean, 0 )] // will skip the Target-Space step
	[Instructions(""), Artist("")]
	static Task Slow2(TargetSpaceCtx _) => Task.CompletedTask;


	[SpiritCard("Fast-0",0),Fast]
	[FromPresence( Filter.Ocean, 0 )] // will skip the Target-Space step
	[Instructions(""), Artist("")]
	static Task Fast0(TargetSpaceCtx _) => Task.CompletedTask;

	[SpiritCard("Fast-1",1),Fast]
	[FromPresence( Filter.Ocean, 0 )] // will skip the Target-Space step
	[Instructions(""), Artist("")]
	static Task Fast1(TargetSpaceCtx _) => Task.CompletedTask;

	[SpiritCard("Fast-2",2),Fast]
	[FromPresence( Filter.Ocean, 0 )] // will skip the Target-Space step
	[Instructions(""), Artist("")]
	static Task Fast2(TargetSpaceCtx _) => Task.CompletedTask;

}