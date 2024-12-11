using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.EmberEyed;

public class EmberEyed_Tests {

	[Trait("spirit","eeb")]
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task UnrelentingStrides_DoubleRise(bool empowered) {
		var eeb = new EmberEyedBehemoth();
		var gs = new SoloGameState(eeb);

		// Given is/isnot empowered
		eeb.Presence.Incarna.Empowered = empowered;
		gs.Board[5].ScopeSpace.Init(eeb.Presence.Incarna,1);

		//   And: has enough elements to trigger innate
		eeb.Configure().Elements("2 fire,1 earth");

		//   And: in slow phase
		gs.Phase = Phase.Slow;

		await eeb.SelectAndResolveActions(gs).AwaitUser(user => {

			// And: can Stomp or Rise
			user.NextDecision.HasPrompt(slowPrompt)
				.HasOptions(innate+","+rise1+",Done")
				// When: Choose Rise
				.Choose(rise1);
			user.NextDecision.HasPrompt("Move/Push (1)").ChooseFirst();

			// Then: can still Stomp or Rise
			user.NextDecision.HasPrompt(slowPrompt)
				.HasOptions(innate+","+rise2+",Done")
				// When: Choose Rise again
				.Choose(rise2);
			user.NextDecision.HasPrompt("Move/Push (1)").ChooseFirst();

			// Then: Innate is gone, we are all done.
		}).ShouldComplete();
	}

	const string slowPrompt = "Select Slow to resolve";
	const string innate = "Smash, Stomp, and Flatten";
	const string rise1 = "The Behemoth Rises";
	const string rise2 = "The Behemoth Rises";

	[Trait("spirit", "eeb")]
	[Theory]
	[InlineData(innate,rise1, null)]
	[InlineData(rise1, innate, null )]
	[InlineData(rise1, innate, innate )]
	[InlineData(innate, rise1, innate)]
	[InlineData(innate, innate, rise1)]
	public async Task UnrelentingStrides_InnateCancels2ndRise(string first, string second, string third) {
		var eeb = new EmberEyedBehemoth();
		var gs = new SoloGameState(eeb);

		// Given is/isnot empowered
		eeb.Presence.Incarna.Empowered = third is not null;
		gs.Board[5].ScopeSpace.Init(eeb.Presence.Incarna, 1);

		//   And: has enough elements to trigger innate
		eeb.Configure().Elements("2 fire,1 earth");

		//   And: in slow phase
		gs.Phase = Phase.Slow;

		const string slowPrompt = "Select Slow to resolve";

		await eeb.SelectAndResolveActions(gs).AwaitUser(user => {
			// When: Choose 1st action
			ChoosePower(user,first);

			//  And: Choose 2nd action
			ChoosePower(user,second);

			//  And: optionally selecting third
			if( third is not null )
				ChoosePower(user,third);

			// Then: Stomp is gone, we are all done.
		}).ShouldComplete();

		static void ChoosePower(VirtualUser user, string choice) {
			user.NextDecision.HasPrompt(slowPrompt).Choose(choice);
			if( choice == rise1 ) 
				user.NextDecision.HasPrompt("Move/Push (1)").ChooseFirst();
		}
	}

}
