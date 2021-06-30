using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;
using SpiritIsland.Base;

namespace SpiritIslandCmd {

	public class CmdLinePlayer {

		readonly SinglePlayerGame game;
		readonly Dictionary<string,Action> commandMap;

		bool shouldRun = true;

		public CmdLinePlayer(SinglePlayerGame game) {

			this.game = game;

			// Init UI
			commandMap = new Dictionary<string, Action> {
				["spirit"] = ShowSpirit,
				["island"] = ShowIsland,
				["invaders"] = ShowInvaders,
				["cards"] = ShowCards,
				["?"] = ShowCommands,
				["q"] = Quit,
			};

		}

		UiMap uiMap;

		public void Play(){

			while(shouldRun && game.Decision != null){

				uiMap = new UiMap(game);
				Console.WriteLine(uiMap.ToPrompt());
				Console.Write("\r\nSI > ");
				string cmd = Console.ReadLine().ToLower().Trim();
				var option = uiMap.GetOption(cmd);
				if(option != null)
					game.Decision.Select(option);
				else
					Generic(cmd); // UI
			}
		}

		bool Generic(string cmd){

			if( !commandMap.ContainsKey(cmd) ) return false;
			commandMap[cmd]();
			return true;
		}

		void Quit() {
			shouldRun = false;
		}

		void ShowCards() {
			var cards = game.Spirit.Hand
				.Union( game.Spirit.PurchasedCards )
				.Union( game.Spirit.DiscardPile )
				.Cast<IActionFactory>()
				.Union( game.Spirit.UnresolvedActionFactories )
				.Distinct()
				.OrderBy( x => x.Speed == Speed.Growth ? 0 : x.Speed == Speed.Fast ? 1 : 2 )
				.ThenBy( x => x.Name )
				.ToList();
			int maxNameWidth = cards.Select( c => c.Name.Length ).Max();
			Console.WriteLine( "Cards:" );
			foreach(var card in cards)
				Console.WriteLine( "\t" + uiMap.FormatFactory( card, maxNameWidth ) );
			Console.WriteLine();
		}

		void ShowSpirit() {
			Console.WriteLine( $"Spirit: {game.Spirit.Text}" );
			// Growth Options
			// !!! Energy Track
			Console.WriteLine( $"\tEnergy: {game.Spirit.EnergyPerTurn}/turn   Total:{game.Spirit.Energy}");
			// !!! Card Track
			Console.WriteLine( $"\tCards:  {game.Spirit.NumberOfCardsPlayablePerTurn}/turn" );
			// !!! Innate Powers
			// Special Rules
			Console.WriteLine();
		}

		void ShowIsland() {
			Console.WriteLine( "Island:" );
			foreach(var board in game.GameState.Island.Boards) {
				foreach(var space in board.Spaces)
					Console.WriteLine( "\t" + uiMap.FormatSpace( space ) );
			}
			Console.WriteLine();
		}

		void ShowInvaders() {
			var d = game.GameState.InvaderDeck;
			Console.WriteLine( "Invaders" );
			Console.WriteLine( "\tDiscard: " + d.CountInDiscard );
			Console.WriteLine( "\tRavage:  " + d.Ravage?.Text );
			Console.WriteLine( "\tBuild:   " + d.Build?.Text );
			Console.WriteLine( "\tExplore: " + "???" );
			//					Console.WriteLine("\tRemaining:"+invaderDeck.);
			Console.WriteLine();
		}

		void ShowCommands(){
			Console.WriteLine("Commands:");
			foreach(string key in commandMap.Keys)
				Console.WriteLine("\t"+key);
			Console.WriteLine();
		}

	}


}
