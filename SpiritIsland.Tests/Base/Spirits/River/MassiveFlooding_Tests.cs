using System;
using System.Linq;
using SpiritIsland.Base;
using SpiritIsland.Core;
using SpiritIsland.SinglePlayer;
using Xunit;

namespace SpiritIsland.Tests.Base.Spirits.River {

	public class MassiveFlooding_Tests {

		readonly SinglePlayerGame game;

		public MassiveFlooding_Tests(){
			// Given: River
			var gameState = new GameState( new RiverSurges() ) {
				Island = new Island( Board.BuildBoardA() )
			};
			gameState.InvaderDeck = InvaderDeck.Unshuffled();
			game = new SinglePlayerGame( gameState );

		}

		// Not enought elements -> nothing
		[Fact]
		public void InsufficientElements() {

			Game_SelectGrowthOption( 0 ); // reclaim
			Game_SelectPowerCardsDone();

			//   And: in slow phase

			//  Then: no massive flooding in Unresolved list
			Assert.Empty( game.Spirit.UnresolvedActionFactories );

		}

		[Fact]
		public void Level1_Pushes1TownOrExplorer() { // 1-Sun, 2-Water

			Game_SelectGrowthOption( 1 ); // PP-1 PP-1

			Game_SelectOption("Select Growth to resolve","PlacePresence(1)");			
			Game_SelectOption("PlacePresence(1) - Select Presence to place","Energy");
			Game_SelectOption("PlacePresence(1) - Where would you like","A4");

			Game_SelectOption("Select Growth to resolve","PlacePresence(1)");			
			Game_SelectOption("PlacePresence(1) - Select Presence to place","Card");
			Game_SelectOption("PlacePresence(1) - Where would you like","A2");

			Game_SelectPowerCards( FlashFloods.Name ); // fast
			Game_SelectPowerCards( RiversBounty.Name );// slow

			Game_DoneWith(Speed.Fast);

			Game_SelectOption("Select Slow to resolve","Massive Flooding");
			Game_SelectOption("Massive Flooding - Select target","A8"); // always a town on A8
			Game_SelectOption("Massive Flooding - Select invader to push","T@2");
			Game_SelectOption("Massive Flooding - Select land to push T@2 to.","A5");
		}

		[Fact]
		public void Level2_2DamagePush3TownOrExplorers() { // 2-Sun, 3-Water

			Given_SpiritCardPlayCount( 3 );
			Given_SpiritGetMoney( 5 );

			Game_SelectGrowthOption( 0 ); // Reclaim

			Game_SelectPowerCards( FlashFloods.Name ); // fast - sun, water
			Game_SelectPowerCards( RiversBounty.Name ); // slow - sun, water, animal
			Game_SelectPowerCards( BoonOfVigor.Name ); // fast - sun, water, plant

			Game_DoneWith( Speed.Fast );

			Game_SelectOption( "Select Slow to resolve", "Massive Flooding" );
			Game_SelectOption( "Massive Flooding - Select target space", "A8");
			Game_SelectOption( "Massive Flooding - Select Innate option", "2 damage, Push up to 3 explorers and/or towns");

			Game_SelectOption( "Massive Flooding - Select land to push E@1 to", "A5" ); // always a town on A8
//			Game_SelectOption( "Massive Flooding - Select invader to push", "T@2" );
//			Game_SelectOption( "Massive Flooding - Select land to push T@2 to.", "A5" );
		}

		void Given_SpiritCardPlayCount( int target ) {
			while(game.Spirit.NumberOfCardsPlayablePerTurn < target)
				game.Spirit.RevealedCardSpaces++;
		}

		void Given_SpiritGetMoney( int target ) {
			while(game.Spirit.EnergyPerTurn < target)
				game.Spirit.RevealedEnergySpaces++;
		}

		[Fact]
		public void Level3() { // 3-Sun, 4-Water, 1-Earth

		}

		void Game_DoneWith( Speed speed ) {
			Game_SelectOption($"Select {speed} to resolve","Done");
		}

		void Game_SelectPowerCardsDone() => Game_SelectPowerCards("Done");

		void Game_SelectPowerCards( string text ) => Game_SelectOption("Buy power cards:", text);

		void Game_SelectGrowthOption( int optionIndex ) {
			Assert.Equal( "Select Growth Option", game.Decision.Prompt );
			game.Decision.Select( game.Decision.Options[optionIndex] );
		}

		void Game_SelectOption( string prompt, string optionText ) {

			if(!game.Decision.Prompt.StartsWith(prompt))
				Assert.Equal(prompt,game.Decision.Prompt);

			var option = game.Decision.Options.FirstOrDefault(o=>o.Text == optionText);
			if(option==null)
				throw new Exception($"option ({optionText} not found in "
					+game.Decision.Options.Select(x=>x.Text).Join(", ")
				);
			game.Decision.Select( option );
		}


		// 2 sun, 3 water => Instead, 2 damage, Push up to 3 explorers and/or towns

		// 3 sun, 4 water, 1 earth => Instead, 2 damage to each invader

		// !!!!!!!!!   Rivers special rules -> presence in water is SS

	}

}
