using Shouldly;
using SpiritIsland.Basegame;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.BringerNS {

	public class DreadApparitions_Tests {

		readonly Board board;
		readonly TargetSpaceCtx ctx;
		public DreadApparitions_Tests() {
			Bringer spirit = new Bringer();
			board = Board.BuildBoardA();
			GameState gs = new GameState( spirit, board );;
			ctx = new TargetSpaceCtx( spirit, gs, board[5] );
		}

		[Fact]
		public void DirectFear_GeneratesDefend() {
			
			async Task When(){
				// Given: using Dread Apparitions
				await DreadApparitions.ActAsync(ctx);
				// When: generating 2 fear
				ctx.AddFear(2);
			}
			_ = When();

			ctx.GameState.GetDefence(ctx.Target).ShouldBe(2);
		}

		[Fact]
		public void TownDamage_Generates2Defend() {
			// has town
			ctx.Invaders.Adjust(InvaderSpecific.Town,1);

			async Task When() {
				// Given: using Dread Apparitions
				await DreadApparitions.ActAsync( ctx );
				// When: destroying the town
				await ctx.InvadersOn.Destroy(Invader.Town,1);
			}
			_ = When();

			// Then: 2 fear should have triggered 2 defend
			ctx.GameState.GetDefence( ctx.Target ).ShouldBe( 2 );

		}

		// Generate 5 DATD fear by 'killing' a city - should defend 5
		[Fact]
		public void CityDamage_Generates5Defend() {
			// has city
			ctx.Invaders.Adjust( InvaderSpecific.City, 1 );

			async Task When() {
				// Given: using Dread Apparitions
				await DreadApparitions.ActAsync( ctx );
				// When: destroying the city
				await ctx.InvadersOn.Destroy( Invader.City, 1 );
			}
			_ = When();

			// Then: 5 fear should have triggered 2 defend
			ctx.GameState.GetDefence( ctx.Target ).ShouldBe( 5 );

		}

		[Fact]
		public void DahanDamage_Generates0() {
			// has 1 city and lots of dahan
			ctx.Invaders.Adjust( InvaderSpecific.City, 1 );
			ctx.AdjustDahan(10);

			async Task When() {
				// Given: using Dread Apparitions
				await DreadApparitions.ActAsync( ctx );

				// When: dahan destroy the city
				await ctx.GameState.Ravage(new InvaderCard(ctx.Target.Terrain));
			}
			_ = When();

			// Then: 2 fear from city
			Assert_GeneratedFear(2); // normal
			// but no defend bonus
			ctx.GameState.GetDefence( ctx.Target ).ShouldBe( 0 );

		}

		[Fact]
		public void FearInOtherLand_Generates0() {
			// has 1 city and lots of dahan
			ctx.Invaders.Adjust( InvaderSpecific.City, 1 );
			ctx.AdjustDahan( 10 );

			async Task When() {
				// Given: using Dread Apparitions
				await DreadApparitions.ActAsync( ctx );

				// When: Power causes fear in a different land
				ctx.GameState.AddFearDirect(new FearArgs { space = board[1], cause = Cause.Power, count = 6 } );
			}
			_ = When();

			// but no defend bonus
			ctx.GameState.GetDefence( ctx.Target ).ShouldBe( 0 );

		}


		void Assert_GeneratedFear( int expectedFearCount ) {
			ctx.GameState.FearPool.ShouldBe( expectedFearCount % 4 );
			ctx.GameState.ActivatedFearCards.Count.ShouldBe( expectedFearCount / 4 );
		}


	}



}
