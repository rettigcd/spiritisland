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
			ctx.Tokens.Add(Invader.Town);

			async Task When() {
				// Given: using Dread Apparitions
				await DreadApparitions.ActAsync( ctx );
				// When: destroying the town
				await ctx.PowerInvaders.Destroy(Invader.Town,1);
			}
			_ = When();

			// Then: 2 fear should have triggered 2 defend
			ctx.GameState.GetDefence( ctx.Target ).ShouldBe( 2 );

		}

		// Generate 5 DATD fear by 'killing' a city - should defend 5
		[Fact]
		public async Task CityDamage_Generates5Defend() {
			// has city
			ctx.Tokens.Add( Invader.City );

			await DreadApparitions.ActAsync( ctx );
			// When: destroying the city
			await ctx.PowerInvaders.Destroy( Invader.City, 1 );

			// Then: 5 fear should have triggered 2 defend
			ctx.GameState.GetDefence( ctx.Target ).ShouldBe( 5 );

		}

		[Fact]
		public async Task DahanDamage_Generates0() {
			// has 1 city and lots of dahan
			ctx.Tokens.Add( Invader.City ); // don't use ctx.Invaders because it has a fake/dream invader count
			ctx.AdjustDahan(10);

			// Given: using Dread Apparitions
			await DreadApparitions.ActAsync( ctx );

			// When: dahan destroy the city
			await ctx.GameState.Ravage( new InvaderCard( ctx.Target.Terrain ) );

			// Then: 2 fear from city
			Assert_GeneratedFear(2); // normal
			// but no defend bonus
			ctx.GameState.GetDefence( ctx.Target ).ShouldBe( 0 );

		}

		[Fact]
		public void FearInOtherLand_Generates0() {
			// has 1 city and lots of dahan
			ctx.Tokens.Add( Invader.City );
			ctx.AdjustDahan( 10 );

			async Task When() {
				// Given: using Dread Apparitions
				await DreadApparitions.ActAsync( ctx );

				// When: Power causes fear in a different land
				ctx.GameState.Fear.AddDirect(new FearArgs { space = board[1], cause = Cause.Power, count = 6 } );
			}
			_ = When();

			// but no defend bonus
			ctx.GameState.GetDefence( ctx.Target ).ShouldBe( 0 );

		}


		void Assert_GeneratedFear( int expectedFearCount ) {

			int actualFear = ctx.GameState.Fear.Pool
				+ 4 * ctx.GameState.Fear.ActivatedCards.Count;

			actualFear.ShouldBe(expectedFearCount,"fear count is wrong");

//			ctx.GameState.Fear.Pool.ShouldBe( expectedFearCount % 4 );
//			ctx.GameState.Fear.ActivatedCards.Count.ShouldBe( expectedFearCount / 4 );
		}


	}



}
