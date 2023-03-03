using SpiritIsland.Log;

namespace SpiritIsland.Tests.Major;

[Trait( "Invaders", "Ravage" )]
[Trait( "Invaders", "Build" )]
[Trait( "Invaders", "Explore" )]
public class ParalyzingFright_Tests {

	// ! should also test without meeting element threshold

	[Fact]
	public async Task StopsAllInvaderActions() {
		List<string> invaderLog = new List<string>();

		var (user, ctx) = TestSpirit.StartGame( PowerCard.For<ParalyzingFright>(), (Action<GameState>)((gs)=>{ 
			var jungleCard = SpiritIsland.InvaderCard.Stage1( Terrain.Jungle);
			gs.InitTestInvaderDeck( (InvaderCard)jungleCard, (InvaderCard)jungleCard, (InvaderCard)jungleCard, (InvaderCard)jungleCard );
			gs.NewLogEntry += (s) => invaderLog.Add( s.Msg());
		}) );

		// Given: there is a ravage card
		user.Grows();
		user.IsDoneBuyingCards();
		invaderLog.Clear();

		// and: there is a space a space that IS-RAVAGE AND BUILD (aka: Jungle - see above)
		user.WaitForNext();
		var spaceCtx = ctx.GameState.Spaces_Unfiltered
			.Select( x=>ctx.Target(x.Space) )
			.Last( s => s.MatchesRavageCard && s.MatchesBuildCard ); // last stays away from city and ocean
		invaderLog.Add("Selected target:"+spaceCtx.Space.Label );

		// And: we have a SS in that land
		ctx.Self.Given_HasPresenceOn(spaceCtx.Space, 2 );

		//  And: it has 3 explorers
		spaceCtx.Tokens.InitDefault( Human.Explorer, 3 );
		spaceCtx.Tokens.InitDefault( Human.Town    , 0 );
		spaceCtx.Tokens.InitDefault( Human.City    , 0 ); // if we had to advance cards, might have buit a city
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "3E@1", "Unable to init to 3 exploreres." );
		//  And 2 dahan
		spaceCtx.Dahan.Init(2);

		// When: grows and purchases card
		user.Grows();
		user.PlaysCard( ParalyzingFright.Name );

		//  When: Activates Card
		user.SelectsFastAction( ParalyzingFright.Name );
		user.TargetsLand_IgnoreOptions( spaceCtx.Space.Label );

		//   Accept the fear card:
		user.ActivateFear();

		// Then: nothing changed
		spaceCtx.Tokens.InvaderSummary().ShouldBe( "3E@1", "should be same that we started with" );

		// Make sure that we actually executed the Ravage Build / Explore Bit
		invaderLog.Count(s=>s.Contains("Exploring")).ShouldBeGreaterThan(0);
	}

}