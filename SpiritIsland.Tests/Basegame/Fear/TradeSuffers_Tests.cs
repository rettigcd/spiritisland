using Shouldly;
using SpiritIsland.Basegame;
using System.Collections.Generic;
using Xunit;
namespace SpiritIsland.Tests.Basegame.Fear {

	public class TradeSuffers_Tests {

		public TradeSuffers_Tests() {
			// On Board-A,
			// use A7 (Sands-2 Dahan)
			// or A4 (Sands-no dahan)
			var (user, ctx) = TestSpirit.SetupGame( PowerCard.For<RiversBounty>(), gs => {
				var fear = gs.Fear;
				gs.InvaderDeck = AvoidTheDahan_Tests.MountainThenAllSands();
				gs.NewLogEntry += (s) => log.Add(s.Msg);
			} );
			this.user = user;
			this.ctx = ctx;
		}

		[Fact]
		public void Level1_CityIsNotDamagedDuringRavage_NoBuild() {

			// Disable destroying presence
			ctx.GameState.DetermineAddBlightEffect = (gs,space) => new AddBlightEffect { Cascade=false,DestroyPresence=false };

			// Invaders do not Build in lands with City.

			// Fill all Invaders spaces with the A7 card
			ClearBlightAndDoNothing();
			ClearBlightAndDoNothing();

			ActivateFearCard( new TradeSuffers() );

			// Given: 1 city and nothing else
			var spaceCtx = ctx.TargetSpace( "A7" );
			spaceCtx.Tokens.Init("1C@3");

			// When: activating fear
			ClearBlightAndDoNothing();
			user.AcknowledgesFearCard( FearCard );

			// Ravage: no dahan, no change:			1B@1,1C@3
			// Build: City present => no build		1B@1,1C@3
			// Explore: +1							1B@1,1C@3,1E@1
			spaceCtx.Tokens.Summary.ShouldBe( "1B,1C@3,1E@1" );
		}

		private const string FearCard = "Trade Suffers : 1 : Invaders do not Build in lands with City.";

		[Fact]
		public void Level1_CityDestroyedDuringRavage_Build() {

			// Disable destroying presence
			ctx.GameState.DetermineAddBlightEffect = (gs,space) => new AddBlightEffect { Cascade=false,DestroyPresence=false };

			// Fill all Invaders spaces with the A7 card
			ClearBlightAndDoNothing();
			ClearBlightAndDoNothing();

			ActivateFearCard( new TradeSuffers() );

			// Given: 1 city and a enough dahan to kill the city but not the last explorer
			var spaceCtx = ctx.TargetSpace( "A7" );
			spaceCtx.Tokens.Init("1C@3,4D@2,2E@1");

			// When: activating fear
			ClearBlightAndDoNothing();
			user.AcknowledgesFearCard( FearCard );

			// Ravage-a: 1 city + 2 explorers do 5 damage killing 2 dahan    1B@1,1C@1,2D@2,2E@1
			// Ravage-b: 2 dahan do 4 damage killing city and 1 explorer     1B@1,2D@2,1E@1 
			// Build: no city present => build		1B@1,2D@2,1E@1,1T@2
			// Explore: +1							1B@1,2D@2,2E@1,1T@2
			spaceCtx.Tokens.Summary.ShouldBe( "1B,2D@2,2E@1,1T@2" );

		}


		void ClearBlightAndDoNothing() {

			// So it doesn't cascade during ravage
			foreach(var space in ctx.AllSpaces) {
				var tmpCtx = ctx.Target(space);
				while(tmpCtx.HasBlight)
					tmpCtx.RemoveBlight().Wait();
			}

			user.DoesNothingForARound();
		}

		void ActivateFearCard(IFearOptions fearCard) {
			ctx.GameState.Fear.Deck.Pop();
			ctx.GameState.Fear.ActivatedCards.Push( new PositionFearCard{ FearOptions=fearCard, Text="FearCard" } );
		}

		readonly VirtualTestUser user;
		readonly SelfCtx ctx;
		readonly List<string> log = new();

	}

}
