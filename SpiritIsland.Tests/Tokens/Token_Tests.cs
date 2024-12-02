using SpiritIsland.NatureIncarnate;
using System.Configuration;

namespace SpiritIsland.Tests;

public class Token_Tests {

	static bool IsInPlay(SpaceSpec space) => !space.IsOcean;

	[Fact]
	public void SummariesAreUnique() {
		var tokens = new ISpaceEntity[] {
			StdTokens.Explorer,
			StdTokens.Town1,StdTokens.Town,
			StdTokens.City1,StdTokens.City2,StdTokens.City,
			StdTokens.Dahan1,StdTokens.Dahan,
			StdTokens.Disease,
			Token.Blight, // conflict with Beast
			Token.Defend,
			Token.Beast,
			Token.Wilds
		};

		var conflicts = tokens
			.GroupBy( t => t.ToString() )
			.Where( grp => grp.Count() > 1 )
			.Select( grp => grp.Key + " is used for:" + grp.OfType<IToken>().Select( t => t.Class.Label + ":" + (t is HumanToken ht ? ht.RemainingHealth : 0) ).Join( ", " ) )
			.Join( "\r\n" );

		conflicts.ShouldBe( "" );
	}

	[Trait("Token","Wilds")]
	[Fact]
	public async Task Wilds_Stops_Explore() {
		var gs = new SoloGameState();

		// Given: a space with no invaders
		Space space = ActionScope.Current.Spaces_Unfiltered.First( s=>IsInPlay(s.SpaceSpec) && !s.HasInvaders() );
		//   And: 1 neighboring town
		var neighbor = space.Adjacent.First();
		neighbor.AdjustDefault( Human.Town, 1 );
		//   And: 1 wilds there
		space.Wilds.Init(1);

		//  When: we explore there
		await space.SpaceSpec.When_CardExplores();

		//  Then: still no invaders
		space.HasInvaders().ShouldBeFalse("there should be no explorers in "+space.SpaceSpec.Label);
		//   And: no wilds here
		(space.Wilds.Count>0).ShouldBeFalse("wilds should be used up");

	}

	[Trait( "Token", "Disease" )]
	[Fact]
	public async Task Disease_Stops_Build() {
		var gs = new SoloGameState();

		// Given: a space with ONLY 1 explorer
		Space space = ActionScope.Current.Spaces_Unfiltered.First( s => IsInPlay(s.SpaceSpec) && !s.HasInvaders() ); // 0 invaders
		space.Given_HasTokens("1E@1,1Z");

		//  When: we build there
		await space.SpaceSpec.When_CardBuilds();

		//  Then: still no towns (just original explorer)
		space.Assert_HasInvaders( "1E@1" ); // "should be no town on "+space.Label
		//   And: no disease here
		space.Disease.Any.ShouldBeFalse( "disease should be used up" );

	}

	[Fact]
	public async Task Incarna_CanMoveInTwoDirections() {
		// The way Space Tracking *was* set up,
		// This works (A3 => rewind => A8): 1st removes from A3, then adds to A8
		// Does NOT work (A8 => rewind => A3):  1st add to A3 (ERROR - token is still on A8, must remove from A8 1st)

		const string FirstPosition = "A3";
		const string SecondPosition = "A8";

		// Given: Game
		var gs = new SoloGameState(new BreathOfDarknessDownYourSpine());
		IHaveMemento mementoHolder = gs;
		gs.Initialize();

		//   And: Incarna saved at 1st location 
		gs.Spirit.Incarna.Space.Label.ShouldBe( FirstPosition );
		object firstSavedState = mementoHolder.Memento;

		//   And: Incarna Moves to 2nd location and is saved
		await new MoveIncarnaAnywhere().ActAsync(gs.Spirit).AwaitUser(u => { 
			u.NextDecision.HasPrompt("Select space to place Incarna.").Choose(SecondPosition);
		});
		gs.Spirit.Incarna.Space.Label.ShouldBe(SecondPosition);
		object secondSavedState = mementoHolder.Memento;

		//   And: Incarna Moves back to 1st position
		await new MoveIncarnaAnywhere().ActAsync(gs.Spirit).AwaitUser(u => {
			u.NextDecision.HasPrompt("Select space to place Incarna.").Choose(FirstPosition);
		});
		gs.Spirit.Incarna.Space.Label.ShouldBe(FirstPosition);

		// -- Test going from Low(A3) to High(A8) --

		//  When: Rewind to 2nd position
		mementoHolder.Memento = secondSavedState;
		//  Then: Incarna back in 2nd position.
		gs.Spirit.Incarna.Space.Label.ShouldBe( SecondPosition );

		// -- Test going from High(A8) to Low(A3) --

		//  When: Rewind to 1st position
		mementoHolder.Memento = firstSavedState;
		//  Then: Incarna back in 1st position.
		gs.Spirit.Incarna.Space.Label.ShouldBe(FirstPosition);

	}

}