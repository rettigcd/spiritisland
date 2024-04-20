namespace SpiritIsland.Tests.Major;

public class IndomitableClaim_Tests {

	// ! should also test without meeting element threshold

	[Trait( "Invaders", "Ravage" )]
	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Build" )]
	[Fact]
	public void StopsAllInvaderActions() {
		List<string> invaderLog = [];

		var (user, self, _) = TestSpirit.StartGame( PowerCard.For(typeof(IndomitableClaim)), (Action<GameState>)((gs)=>{ 
			var jungleCard = SpiritIsland.InvaderCard.Stage1( Terrain.Jungle);
			gs.InitTestInvaderDeck( (InvaderCard)jungleCard, (InvaderCard)jungleCard, (InvaderCard)jungleCard, (InvaderCard)jungleCard );
			gs.NewLogEntry += (s) => invaderLog.Add( s.Msg());
		}) );

		// Given: there a ravage card
		user.Grows();
		user.IsDoneBuyingCards();
		invaderLog.Clear();
		user.WaitForNext();

		// and: there is a space that IS-RAVAGE AND BUILD (aka: Jungle - see above)
		TargetSpaceCtx spaceCtx = ActionScope.Current.Spaces_Unfiltered
			.Select( x=>self.Target(x.SpaceSpec) )
			.Last( s => s.MatchesRavageCard && s.MatchesBuildCard ); // last stays away from city and ocean
		invaderLog.Add("Selected target:"+spaceCtx.SpaceSpec.Label );

		// And: we have a presence in that land
		self.Given_IsOn( spaceCtx.Space );

		//  And: it has 3 explorers
		spaceCtx.Space.InitDefault( Human.Explorer, 3 );
		spaceCtx.Space.InitDefault( Human.Town, 0 );
		spaceCtx.Space.InitDefault( Human.City, 0 ); // if we had to advance cards, might have buit a city
		spaceCtx.Space.InvaderSummary().ShouldBe( "3E@1", "Unable to init to 3 exploreres." );
		//  And 2 dahan
		spaceCtx.Dahan.Init(2);

		// When: grows and purchases card
		user.Grows();
		user.PlaysCard( IndomitableClaim.Name );

		//  And: has enough elements to trigger the bonus
		self.Elements[Element.Sun] = 2;
		self.Elements[Element.Earth] = 3;

		//  When: Activates Card
		user.SelectsFastAction( IndomitableClaim.Name );
		user.TargetsLand_IgnoreOptions( spaceCtx.SpaceSpec.Label );
		user.PullsPresenceFromTrack(self.Presence.Energy.RevealOptions.Single());

		// Then: nothing changed
		spaceCtx.Space.InvaderSummary().ShouldBe( "3E@1", "should be same that we started with" );

		// Make sure that we actually executed the Ravage Build / Explore Bit
		invaderLog.Count(s=>s.Contains("Exploring")).ShouldBeGreaterThan(0);
	}

}