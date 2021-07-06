using System;
using System.Linq;
using SpiritIsland.SinglePlayer;
using Xunit;

namespace SpiritIsland.Tests {
	public class GameBaseTests {

		protected SinglePlayerGame game;

		protected void Game_DoneWith( Speed speed ) {
			Game_SelectOption($"Select {speed} to resolve","Done");
		}

		protected void Game_SelectGrowthOption( int optionIndex ) {
			Assert.Equal( "Select Growth Option", game.Decision.Prompt );
			game.Decision.Select( game.Decision.Options[optionIndex] );
		}

		protected void Game_SelectOption( string prompt, string optionText ) {

			if(!game.Decision.Prompt.StartsWith(prompt))
				Assert.Equal(prompt,game.Decision.Prompt);

			var option = game.Decision.Options.FirstOrDefault(o=>o.Text == optionText);
			if(option==null)
				throw new Exception($"option ({optionText} not found in "
					+game.Decision.Options.Select(x=>x.Text).Join(", ")
				);
			game.Decision.Select( option );
		}

		protected void Game_BuyPowerCards( string text ) => Game_SelectOption("Buy power cards:", text);

		protected void Game_SelectPowerCardsDone() => Game_BuyPowerCards("Done");

	}

}
