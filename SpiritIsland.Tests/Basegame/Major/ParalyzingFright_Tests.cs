using Shouldly;
using SpiritIsland.Basegame;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame {
	public class ParalyzingFright_Tests {

		// ! should also test without meeting element threshold

		[Fact]
		public void StopsAllInvaderActions() {
			List<string> invaderLog = new List<string>();

			var (user, ctx) = TestSpirit.SetupGame( PowerCard.For<ParalyzingFright>(), (gs)=>{ 
				var jungleCard = new InvaderCard(Terrain.Jungle);
				gs.InvaderDeck = InvaderDeck.BuildTestDeck( jungleCard, jungleCard, jungleCard, jungleCard );
				gs.NewLogEntry += (s) => invaderLog.Add(s.Msg());
			} );

			// Given: there is a ravage card
			user.Grows();
			user.IsDoneBuyingCards();
			invaderLog.Clear();

			// and: there is a space a space that IS-RAVAGE AND BUILD (aka: Jungle - see above)
			var spaceCtx = ctx.AllSpaces
				.Select( ctx.Target )
				.Last( s => s.MatchesRavageCard && s.MatchesBuildCard ); // last stays away from city and ocean
			invaderLog.Add("Selected target:"+spaceCtx.Space.Label );

			// And: we have a SS in that land
			ctx.Self.Presence.PlaceOn(spaceCtx.Space,spaceCtx.GameState);
			ctx.Self.Presence.PlaceOn(spaceCtx.Space,spaceCtx.GameState);

			//  And: it has 3 explorers
			spaceCtx.Tokens.InitDefault( Invader.Explorer, 3 );
			spaceCtx.Tokens.InitDefault( Invader.Town    , 0 );
			spaceCtx.Tokens.InitDefault( Invader.City    , 0 ); // if we had to advance cards, might have buit a city
			spaceCtx.Tokens.InvaderSummary.ShouldBe( "3E@1", "Unable to init to 3 exploreres." );
			//  And 2 dahan
			spaceCtx.Dahan.Init(2);

			// When: grows and purchases card
			user.Grows();
			user.PlaysCard( ParalyzingFright.Name );

			//  When: Activates Card
			user.SelectsFastAction( ParalyzingFright.Name );
			user.TargetsLand_IgnoreOptions( spaceCtx.Space.Label );

			//   Accept the fear card:
			user.SelectsFirstOption("Activating Fear");

			// Then: nothing changed
			spaceCtx.Tokens.InvaderSummary.ShouldBe( "3E@1", "should be same that we started with" );

			// Make sure that we actually executed the Ravage Build / Explore Bit
			invaderLog.Count(s=>s.Contains("Exploring")).ShouldBeGreaterThan(0);
		}

	}


}
