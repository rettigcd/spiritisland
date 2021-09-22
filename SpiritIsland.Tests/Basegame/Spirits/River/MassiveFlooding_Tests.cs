﻿using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class MassiveFlooding_Tests : RiverGame {

		readonly GameState gs;

		public MassiveFlooding_Tests():base(){
			// Given: River
			spirit.UsePowerProgression();
			gs = new GameState( spirit, Board.BuildBoardA() ) {
				InvaderDeck = InvaderDeck.Unshuffled()
			};
			game = new SinglePlayerGame( gs );

		}

		// Not enought elements -> nothing
		[Fact]
		public void InsufficientElements() {

			User.SelectsGrowthOption( 0 );
			User.DrawsPowerCard();
			User.GainsEnergy();
			User.ReclaimsAll();

			User.IsDoneBuyingCards();

			//   And: in slow phase

			//  Then: no massive flooding in Unresolved list
			Assert.Empty( game.Spirit.GetAvailableActions(Speed.Slow) );

		}

		[Fact]
		public void Level1_Pushes1TownOrExplorer() { // 1-Sun, 2-Water

			User.SelectsGrowthOption( 1 ); // PP-1 PP-1

			User.PlacesPresence( "2 energy", "A4" );
			User.PlacesPresence( "2 cardplay", "A2" );

			User.BuysPowerCard( FlashFloods.Name ); // fast
			User.BuysPowerCard( RiversBounty.Name );// slow

			User.IsDoneWith( Speed.Fast );

			User.SelectsSlowAction("River's Bounty,(Massive Flooding)");
			User.TargetsLand( "A2,A3,A5,(A8)" );
			User.PushesTokensTo( "(T@2),E@1", "(A5),A6,A7" );
		}

		[Fact]
		public void Level2_2DamagePush3TownOrExplorers() { // 2-Sun, 3-Water

			Given_SpiritCardPlayCount( 3 );
			Given_SpiritGetMoney( 5 );

			User.SelectsGrowthOption( 0 ); // reclaim
			User.DrawsPowerCard();
			User.GainsEnergy();
			User.ReclaimsAll();

			User.BuysPowerCard( FlashFloods.Name ); // fast - sun, water
			User.BuysPowerCard( RiversBounty.Name ); // slow - sun, water, animal
			User.BuysPowerCard( BoonOfVigor.Name ); // fast - sun, water, plant

			User.IsDoneWith( Speed.Fast );

			User.SelectsSlowAction("River's Bounty,(Massive Flooding)" );
			User.TargetsLand("A5,(A8)");
			User.OptionallyPushesInvaderTo("E@1","(A5),A6,A7");
		}

		[Fact]
		public void Level3_2DamageToEachInvader() { // 3 sun, 4 water, 1 earth
			// Instead, 2 damage to each invader

			Given_SpiritCardPlayCount( 4 );
			Given_SpiritGetMoney( 5 );

			// 1 city, 5 towns, 5 invaders on A4 (range 1 from SS)
			var space = game.Spirit.SacredSites.First().Adjacent.First();
			var grp = game.GameState.Tokens[space];
			grp.Adjust( Invader.City.Default, 1);
			grp.Adjust( Invader.Town.Default, 5 );
			grp.Adjust( Invader.Explorer.Default, 5 );

			User.SelectsGrowthOption( 0 ); // Reclaim
			User.DrawsPowerCard();
			User.GainsEnergy();
			User.ReclaimsAll();

			User.BuysPowerCard( FlashFloods.Name );  // fast - sun, water
			User.BuysPowerCard( RiversBounty.Name ); // slow - sun, water, animal
			User.BuysPowerCard( BoonOfVigor.Name );  // fast - sun, water, plant
			User.BuysPowerCard( WashAway.Name );     // slow -      water, earth

			User.IsDoneWith( Speed.Fast );

			User.SelectsSlowAction("River's Bounty,Wash Away,(Massive Flooding)" );
			User.TargetsLand( "(A1),A5,A8" );
			
			System.Threading.Thread.Sleep(50);
			game.GameState.Assert_Invaders(space, "1C@1" );
		}


		void Given_SpiritCardPlayCount( int target ) {
			while(game.Spirit.NumberOfCardsPlayablePerTurn < target)
				game.Spirit.Presence.CardPlays.RevealedCount++;
		}

		void Given_SpiritGetMoney( int target ) {
			while(game.Spirit.EnergyPerTurn < target)
				game.Spirit.Presence.Energy.RevealedCount++;
		}

	}

}
