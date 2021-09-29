using Shouldly;
using SpiritIsland.Basegame;
using System.Collections.Generic;
using Xunit;
namespace SpiritIsland.Tests.Basegame.Fear {

	// For calculating pre-ravage invaders required to end with known post-ravage invaders/dahan use:
	// Town(start) = Town(end) + Dahan(end)
	// Dahan(start) = Town(start) + Dahan(end)

	public class AvoidTheDahan_Tests {

		#region constructor

		public AvoidTheDahan_Tests() {
			// On Board-A,
			// use A7 (Sands-2 Dahan)
			// or A4 (Sands-no dahan)
			var (user, ctx) = TestSpirit.SetupGame( PowerCard.For<RiversBounty>(), gs => {
				var fear = gs.Fear;
				gs.InvaderDeck = MountainThenAllSands();
				gs.NewInvaderLogEntry += (s) => log.Add(s);
			} );
			this.user = user;
			this.ctx = ctx;
		}

		#endregion

		[Fact]
		public void NullFearCard_NormalExplore() {

			var spaceCtx = ctx.TargetSpace( "A7" );
			spaceCtx.Tokens.Summary.ShouldBe( "2D@2" );

			ActivateFearCard(new NullFearCard());

			ClearBlightAndDoNothing();
			user.AcknowledgesFearCard( "Null Fear Card:1:x" );

			spaceCtx.Tokens.InvaderSummary.ShouldBe( "1E@1" );
		}

		[Fact]
		public void Level1_NoExplore() {

			var spaceCtx = ctx.TargetSpace( "A7" );
			spaceCtx.Tokens.Summary.ShouldBe( "2D@2" );

			ActivateFearCard(new AvoidTheDahan());

			ClearBlightAndDoNothing();
			user.AcknowledgesFearCard( "Avoid the Dahan:1:Invaders do not Explore into lands with at least 2 Dahan." );

			spaceCtx.Tokens.InvaderSummary.ShouldBe("");

		}

		[Fact]
		public void Level1_DahanKilledDuringRavage_ShouldExplore() {

			var spaceCtx = ctx.TargetSpace( "A7" );
			spaceCtx.Tokens.Summary.ShouldBe( "2D@2" );

			ClearBlightAndDoNothing();
			spaceCtx.Tokens.Summary.ShouldBe( "2D@2,1E@1" );

			ClearBlightAndDoNothing();
			spaceCtx.Tokens.Summary.ShouldBe( "2D@2,2E@1,1T@2" );

			// When: activating: 'Avoid the Dahan'
			ActivateFearCard( new AvoidTheDahan() );

			ClearBlightAndDoNothing();
			user.AcknowledgesFearCard( "Avoid the Dahan:1:Invaders do not Explore into lands with at least 2 Dahan." );

			// Ravage should kill both dahan                   => 2 explorers and 1 town
			// Build should make a city                        => 2 explorers, 1 town, 1 city
			// Explore should add an explorer (dahan are gone) => 3 explorers, 1 town, 1 city

			spaceCtx.Tokens.InvaderSummary.ShouldBe( "1C@3,1T@2,3E@1" );

		}

		[Fact]
		public void Level2_3DahanAtFear_1DahanAtBuild_DoBuild() {

			// Fill all Invaders spaces with the A7 card
			ClearBlightAndDoNothing();
			ClearBlightAndDoNothing();

			ActivateFearCard( new AvoidTheDahan() );
			ElevateTerrorLevelTo(2);

			// Given: Starting out Dahan(3) outnumber town/city(2)
			var spaceCtx = ctx.TargetSpace( "A7" );
			spaceCtx.InitTokens( "3D@2,2T@2" );

			// When: activating fear
			ClearBlightAndDoNothing();
			user.AcknowledgesFearCard( "Avoid the Dahan:2:Invaders do not Build in lands where Dahan outnumber Town / City." );

			// Ravage: 2 towns kill 2 dahan leaving 1, 1 dahan kills 1 town leaving 1:  1B@1,1D@2,1T@2

			// Then: Dahan DO NOT outnumber the towns and cities any more! (1 & 1)
			//  But: we DO have some dahan

			// Build: Build city:	1B@1,1C@3,1D@2,1T@2
			// Explore: +1			1B@1,1C@3,1D@2,1E@1,1T@2
			spaceCtx.Tokens.Summary.ShouldBe( "1B@1,1C@3,1D@2,1E@1,1T@2" );
		}

		[Fact]
		public void Level2_NoBuild() {
			// "Invaders do not Build in lands where Dahan outnumber Town / City."

			// Fill all Invaders spaces with the A7 card
			ClearBlightAndDoNothing();
			ClearBlightAndDoNothing();

			ActivateFearCard( new AvoidTheDahan() );
			ElevateTerrorLevelTo(2);

			// Given: Dahan(2) outnumber town/city(0)  + 3 explorer (to enable build)
			var spaceCtx = ctx.TargetSpace( "A7" );
			spaceCtx.InitTokens("2D@2,3E@1");

			// When: activating fear
			ClearBlightAndDoNothing();
			user.AcknowledgesFearCard( "Avoid the Dahan:2:Invaders do not Build in lands where Dahan outnumber Town / City." );

			// Then: no build
			// Ravage: 3 explorers kill 1 dahan, 1 dahan kills 2 explorer:  1B@1,1D@2,1E@1
			// Build: 1 dahan out numbers town/cities (0), no build:  1B@1,1D@2,1E@1
			// Explore: +1   1D@2,2E@1
			spaceCtx.Tokens.Summary.ShouldBe( "1B@1,1D@2,2E@1" );
		}

		[Fact]
		public void Level3_3DahanAtFear_1DahanAtBuild_NoBuild() {

			// Fill all Invaders spaces with the A7 card
			ClearBlightAndDoNothing();
			ClearBlightAndDoNothing();

			ActivateFearCard( new AvoidTheDahan() );
			ElevateTerrorLevelTo( 3 );

			// Given: Starting out Dahan(3) outnumber town/city(2)
			var spaceCtx = ctx.TargetSpace( "A7" );
			spaceCtx.InitTokens( "3D@2,2T@2" );

			// When: activating fear
			ClearBlightAndDoNothing();
			user.AcknowledgesFearCard( "Avoid the Dahan:3:Invaders do not Build in lands with Dahan." );

			// Ravage: 2 towns kill 2 dahan leaving 1, 1 dahan kills 1 town leaving 1:  1B@1,1D@2,1T@2

			// Then: Dahan do not out number cities/towns! (1 & 1)
			//  But: we do have SOME dahan

			// Build: no build      1B@1,1D@2,1T@2
			// Explore: +1			1B@1,1D@2,1E@1,1T@2
			spaceCtx.Tokens.Summary.ShouldBe( "1B@1,1D@2,1E@1,1T@2" );
		}

		[Fact]
		public void Level3_1DahanAtFear_0DahanAtBuild_Build() {

			// Fill all Invaders spaces with the A7 card
			ClearBlightAndDoNothing();
			ClearBlightAndDoNothing();

			ActivateFearCard( new AvoidTheDahan() );
			ElevateTerrorLevelTo(3);

			// Given: Starting out Dahan(3) outnumber town/city(2)
			var spaceCtx = ctx.TargetSpace( "A7" );
			spaceCtx.InitTokens("1D@2,1T@2");

			// When: activating fear
			ClearBlightAndDoNothing();
			user.AcknowledgesFearCard( "Avoid the Dahan:3:Invaders do not Build in lands with Dahan." );

			// Ravage: 1 towns kill 1 dahan leaving 0:  1B@1,1T@2

			// Then: dahan are gone

			// Build: build     1B@1,1C@3,1T@2
			// Explore: +1		1B@1,1C@3,1E@1,1T@2
			spaceCtx.Tokens.Summary.ShouldBe( "1B@1,1C@3,1E@1,1T@2" );
		}

		#region private

		void ElevateTerrorLevelTo( int desiredFearLevel ) {
			while(ctx.GameState.Fear.TerrorLevel < desiredFearLevel)
				ctx.GameState.Fear.Deck.Pop();
		}


		void ClearBlightAndDoNothing() {
			ClearBlight();
			user.DoesNothingForARound();
			System.Threading.Thread.Sleep( 10 );
		}

		static InvaderDeck MountainThenAllSands() {
			var sand = new InvaderCard( Terrain.Sand );
			return InvaderDeck.BuildTestDeck(
				new InvaderCard( Terrain.Mountain ), // initial explorer in mountains
				sand, sand, sand, sand, sand
			);
		}

		void ActivateFearCard(IFearOptions fearCard) {
			ctx.GameState.Fear.Deck.Pop();
			ctx.GameState.Fear.ActivatedCards.Push( new PositionFearCard{ FearOptions=fearCard, Text="FearCard" } );
		}

		void ClearBlight() {
			// So it doesn't cascade during ravage
			foreach(var space in ctx.AllSpaces) {
				var tmpCtx = ctx.TargetSpace(space);
				while(tmpCtx.HasBlight)
					tmpCtx.RemoveBlight();
			}
		}

		readonly VirtualTestUser user;
		readonly SpiritGameStateCtx ctx;
		readonly List<string> log = new();

		#endregion
	}

}
