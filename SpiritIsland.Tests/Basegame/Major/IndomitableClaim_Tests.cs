using Shouldly;
using SpiritIsland.Basegame;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame {

	public class IndomitableClaim_Tests {

		// ! should also test without meeting element threshold

		[Fact]
		public void StopsAllInvaderActions() {
			List<string> invaderLog = new List<string>();

			var (user, ctx) = CardSpirit.SetupGame( PowerCard.For<IndomitableClaim>(), (gs)=>{ 
				var jungleCard = new InvaderCard(Terrain.Jungle);
				gs.InvaderDeck = InvaderDeck.BuildTestDeck( jungleCard, jungleCard, jungleCard, jungleCard );
				gs.NewInvaderLogEntry += (s) => invaderLog.Add(s);
			} );

			// Given: there a ravage card
			user.Grows();
			user.IsDoneBuyingCards();
			invaderLog.Clear();

			// and: there is a space a space that IS-RAVAGE AND BUILD (aka: Jungle - see above)
			var spaceCtx = ctx.AllSpaces
				.Select( ctx.TargetSpace )
				.Last( s => s.MatchesRavageCard && s.MatchesBuildCard ); // last stays away from city and ocean
			invaderLog.Add("Selected target:"+spaceCtx.Space.Label );

			// And: we have a presence in that land
			ctx.Self.Presence.PlaceOn(spaceCtx.Space);

			//  And: it has 3 explorers
			spaceCtx.Tokens[Invader.Explorer.Default] = 3;
			spaceCtx.Tokens[Invader.Town.Default] = 0;
			spaceCtx.Tokens[Invader.City.Default] = 0; // if we had to advance cards, might have buit a city
			spaceCtx.Tokens.InvaderSummary.ShouldBe( "3E@1", "Unable to init to 3 exploreres." );
			//  And 2 dahan
			spaceCtx.Tokens[TokenType.Dahan.Default ] = 2;

			// When: grows and purchases card
			user.Grows();
			user.BuysPowerCard( IndomitableClaim.Name );

			//  And: has enough elements to trigger the bonus
			ctx.Self.Elements[Element.Sun] = 2;
			ctx.Self.Elements[Element.Earth] = 3;

			//  When: Activates Card
			user.SelectsFastAction( IndomitableClaim.Name );
			user.TargetsLand_IgnoreOptions( spaceCtx.Space.Label );
			user.PullsPresenceFromTrack(ctx.Self.Presence.Energy.Next);

			// Then: nothing changed
			System.Threading.Thread.Sleep( 20 );
			spaceCtx.Tokens.InvaderSummary.ShouldBe( "3E@1", "should be same that we started with" );

			// Make sure that we actually executed the Ravage Build / Explore Bit
			invaderLog.Count(s=>s.Contains("Exploring")).ShouldBeGreaterThan(0);
		}

	}


}
