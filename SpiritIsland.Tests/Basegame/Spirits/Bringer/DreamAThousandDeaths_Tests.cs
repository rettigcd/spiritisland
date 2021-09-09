﻿using Shouldly;
using SpiritIsland.Basegame;
using System;
using System.Threading.Tasks;
using Xunit;


namespace SpiritIsland.Tests.Basegame.Spirits.BringerNS {

	public class DreamAThousandDeaths_Tests {

		readonly Board board;
		readonly TargetSpaceCtx ctx;
		public DreamAThousandDeaths_Tests() {
			spirit = new Bringer();
			board = Board.BuildBoardA();
			gs = new GameState( spirit, board );
			ctx = MakeFreshCtx();
		}
		readonly Bringer spirit;
		readonly GameState gs;

		TargetSpaceCtx MakeFreshCtx() => new TargetSpaceCtx( spirit, gs, board[5] );


		// 1: Raging Storm - 1 damage to each invader (slow)
		static readonly Func<TargetSpaceCtx,Task> OneDamageToEachAsync = RagingStorm.ActAsync;
		// 1 & 2: The Jungle Hugers - destroy all explorers and all towns
		static readonly Func<TargetSpaceCtx, Task> DestroyAllExplorersAndTownsAsync = TheJungleHungers.ActAsync;
		// 4, cleansing flood - 4 damage (+10 if you hvae 4 water)
		static readonly Func<TargetSpaceCtx, Task> FourDamage = CleansingFloods.ActAsync;

		[Theory]
		[InlineData( "damage" )]
		[InlineData( "destroy" )]
		public void DreamKilledExplorers_ArePushed(string method) {
			const int count = 2;

			// Given: 2 explorers
			ctx.Tokens.Adjust(Invader.Explorer.Default, count );

			// When: causing 1 damage to each invader
			switch(method) {
				case "damage": _ = OneDamageToEachAsync( ctx ); break;
				case "destroy": _ = DestroyAllExplorersAndTownsAsync( ctx ); break;
			}

			// Then: dream-death allows User pushes them
			for(int i = 0; i < count; ++i) {
				ctx.Self.Action.AssertDecision( "Select item to push (1 remaining)","E@1", "E@1" );
				ctx.Self.Action.AssertDecision( "Push E@1 to", "A1,A4,A6,A7,A8", "A7" );
			}

			// And: 0-fear
			Assert_GeneratedFear( 0 );

			//  and: explorer on destination
			ctx.GameState.Assert_Invaders( board[7], $"{count}E@1" );
			//  and: not at origin
			ctx.PowerInvaders.Counts.InvaderSummary.ShouldBe( "" );
		}

		[Fact]
		public void KillingTown_GeneratesFear_PushesIt() {
			int count = 2;
			// generate 2 fear per town destoryed,
			// pushes town

			// Given: 2 town
			ctx.Tokens.Adjust( Invader.Town.Default, count );

			// When: destorying towns
			_ = DestroyAllExplorersAndTownsAsync( ctx );

			// Then: dream-death allows User pushes them
			for(int i = 0; i < count; ++i) {
				ctx.Self.Action.AssertDecision( "Select item to push (1 remaining)", "T@2", "T@2" );
				ctx.Self.Action.AssertDecision( "Push T@2 to", "A1,A4,A6,A7,A8", "A7" );
			}

			// And:4-fear
			Assert_GeneratedFear( count * 2 );

			//  and: town on destination
			ctx.GameState.Assert_Invaders( board[7], $"{count}T@2" );
			//  and: not at origin
			ctx.PowerInvaders.Counts.InvaderSummary.ShouldBe("");

		}

		[Fact]
		public void DreamDamageResetsEachPower() {

			// Given: 2 explorers
			ctx.Tokens.Adjust( Invader.City.Default, 1 );

			// When: 3 separate actinos cause 1 damage
			async Task Run3Async(){
				await OneDamageToEachAsync( MakeFreshCtx() );
				await OneDamageToEachAsync( MakeFreshCtx() );
				await OneDamageToEachAsync( MakeFreshCtx() );
			}
			_=Run3Async();

			ctx.Self.Action.IsResolved.ShouldBeTrue();

			// And: 0-fear
			Assert_GeneratedFear(0); // city never destroyed
		}


		[Fact]
		public async Task ConsecutivePowersCanDreamKillMultipletimes() {

			// Given: 1 very-damaged city
			ctx.Adjust( Invader.City[1], 1 );

			// When: 3 separate actinos cause 1 damage
			// EACH power gets a fresh ctx so INVADERS can reset
			await OneDamageToEachAsync( MakeFreshCtx() ); 
			await OneDamageToEachAsync( MakeFreshCtx() );
			await OneDamageToEachAsync( MakeFreshCtx() );

			ctx.Self.Action.IsResolved.ShouldBeTrue();

			// And: 0-fear
			Assert_GeneratedFear( 3*5 ); // city never destroyed
			// City still there
			ctx.PowerInvaders[Invader.City[1]].ShouldBe(1);
		}

		[Fact]
		public async Task MaxKillOnce() {
			// Given: 1 very-damaged city
			ctx.Adjust( Invader.City[1], 1 );

			// When: doing 4 points of damage
			await FourDamage( MakeFreshCtx() );

			ctx.Self.Action.IsResolved.ShouldBeTrue();

			// And: 0-fear
			Assert_GeneratedFear( 1 * 5 ); // city only destroyed once

			// City with partial damage still there
			ctx.PowerInvaders[Invader.City[1]].ShouldBe( 1 );
		}

		void Assert_GeneratedFear( int expectedFearCount ) {
			int actualGeneratedFear = ctx.GameState.Fear.Pool
				+ 4 * ctx.GameState.Fear.ActivatedCards.Count;
			actualGeneratedFear.ShouldBe(expectedFearCount,"fear countis wrong");
//			ctx.GameState.Fear.Pool.ShouldBe( expectedFearCount % 4 );
//			ctx.GameState.Fear.ActivatedCards.Count.ShouldBe( expectedFearCount / 4 );
		}

	}
}
