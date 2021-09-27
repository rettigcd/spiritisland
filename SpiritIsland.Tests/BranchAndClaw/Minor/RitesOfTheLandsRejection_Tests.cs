using Shouldly;
using SpiritIsland.BranchAndClaw;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.BranchAndClaw.Minor {
	public class RitesOfTheLandsRejection_Tests {

		[Theory]
		[InlineDataAttribute(false,"1T@2,1E@1")]
		[InlineDataAttribute(true,"1E@1")]
		public void SingleBuild(bool playsCard, string result) {

			var (user,ctx) = CardSpirit.SetupGame( PowerCard.For<RitesOfTheLandsRejection>() );

			// Given: find a space with 1 explorer
			var spaceCtx = ctx.AllSpaces
				.Select( ctx.TargetSpace )
				.First( sc => sc.Tokens.InvaderSummary == "1E@1" );

			//   And: add Dahan (because card requires it)
			spaceCtx.Tokens[TokenType.Dahan[2]] = 1;

			// When: growing
			user.Grows();

			//  And: purchase test card
			user.BuysPowerCard( RitesOfTheLandsRejection.Name );
			if(playsCard) {
				user.SelectsFastAction( RitesOfTheLandsRejection.Name );
				user.TargetsLand_IgnoreOptions( spaceCtx.Space.Label );
				user.AssertDecisionX( "Select Power Option", "{Stop build - 1 fear / (Dahan or T/C)},Push up to 3 dahan", "{}" );
			} else
				//  And: done with fast (no more cards..)
				user.IsDoneWith( Speed.Fast );

			// Then: space should have a building
			System.Threading.Thread.Sleep( 20 );
			spaceCtx.Tokens.InvaderSummary.ShouldBe( result );

		}

		//		[Fact]
		//		public void Has2Builds_NoBuild() {} // !!! how do I construct 2 builds?

	}


}
