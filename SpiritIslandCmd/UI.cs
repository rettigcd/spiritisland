using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;

namespace SpiritIslandCmd {
	class UI {

		readonly Formatter formatter;
		public UI(Formatter formatter){
			this.formatter = formatter;
		}

		public void PhaseComplete(string details){
			Console.WriteLine(details);
		}

		public void ShowPrompt(string prompt){
			Console.WriteLine(prompt);
			Console.Write("\r\nSI > ");
		}

		public void ShowCards( List<IActionFactory> cards ) {
			int maxNameWidth = cards.Select( c => c.Name.Length ).Max();
			Console.WriteLine( "Cards:" );
			foreach(var card in cards)
				Console.WriteLine( "\t" + formatter.Format( card, maxNameWidth ) );
		}

		public void ShowSpirit(Spirit spirit) {
			Console.WriteLine( $"Spirit: {spirit.Text}" );
			// Growth Options
			// !!! Energy Track
			Console.WriteLine( $"\tEnergy: {spirit.EnergyPerTurn}/turn   Total:{spirit.Energy}");
			// !!! Card Track
			Console.WriteLine( $"\tCards:  {spirit.NumberOfCardsPlayablePerTurn}/turn" );
			// !!! Innate Powers
			// Special Rules
		}

		public void ShowIsland(Island island) {
			Console.WriteLine( "Island:" );
			foreach(var board in island.Boards) {
				foreach(var space in board.Spaces)
					Console.WriteLine( "\t" + formatter.Format( space ) );
			}
		}

		public void ShowInvaders(InvaderDeck invaderDeck){
			Console.WriteLine( "Invaders" );
			Console.WriteLine( "\tDiscard: " + invaderDeck.CountInDiscard );
			Console.WriteLine( "\tRavage:  " + invaderDeck.Ravage?.Text );
			Console.WriteLine( "\tBuild:   " + invaderDeck.Build?.Text );
			Console.WriteLine( "\tExplore: " + "???" );
		}

		public void ShowHelp(IEnumerable<string> keys){
			Console.WriteLine("Commands:");
			foreach(string key in keys)
				Console.WriteLine("\t"+key);
		}

	}

}
