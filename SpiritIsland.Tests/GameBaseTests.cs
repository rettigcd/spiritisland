using System;
using System.Linq;
using Shouldly;
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

		protected void Prompt_Select( string prompt, string optionsString, string select, bool done = false ) {
			string msg = $"{prompt}:{optionsString}:{select}";
			game.Decision.Prompt.ShouldBe( prompt, msg );
			game.Decision.Options.Select( x => x.Text ).Join( "," ).ShouldBe( optionsString, msg );
			IOption match = game.Decision.Options.Single( x => x.Text == select );
			game.Decision.Select( match );
			if(done)
				game.Decision.Options.Length.ShouldBe(0, msg );
			else
				game.Decision.Options.Length.ShouldBeGreaterThan( 0, msg );
		}

		protected void Game_SelectOption( string prompt, string optionText ) {

			if(!game.Decision.Prompt.StartsWith(prompt))
				game.Decision.Prompt.ShouldBe(prompt);

			var option = game.Decision.Options.FirstOrDefault(o=>o.Text == optionText);
			if(option==null)
				throw new Exception($"option ({optionText} not found in "
					+game.Decision.Options.Select(x=>x.Text).Join(", ")
				);
			game.Decision.Select( option );
		}

		protected void Game_SelectOption( string prompt, IOption option ) {

			if(!game.Decision.Prompt.StartsWith( prompt ))
				Assert.Equal( prompt, game.Decision.Prompt );

			//var option = game.Decision.Options.FirstOrDefault( o => o.Text == optionText );
			//if(option == null)
			//	throw new Exception( $"option ({optionText} not found in "
			//		+ game.Decision.Options.Select( x => x.Text ).Join( ", " )
			//	);
			game.Decision.Select( option );
		}

		protected void Game_SelectOptionContains( string prompt, string substring ) {

			if(!game.Decision.Prompt.StartsWith( prompt ))
				Assert.Equal( prompt, game.Decision.Prompt );

			var option = game.Decision.Options.FirstOrDefault( o => o.Text.ToLower().Contains(substring.ToLower()) );
			if(option == null)
				throw new Exception( $"option ({substring} not found in "
					+ game.Decision.Options.Select( x => x.Text ).Join( ", " )
				);
			game.Decision.Select( option );
		}


		protected void Game_BuyPowerCards( string text ) => Game_SelectOption("Buy power cards:", text);

		protected void Game_SelectPowerCardsDone() => Game_BuyPowerCards("Done");

	}

}
