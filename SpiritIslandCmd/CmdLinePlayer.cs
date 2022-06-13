using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;
using SpiritIsland.SinglePlayer;

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

			while(shouldRun && game.UserPortal != null){

				uiMap = new UiMap(game);
				Console.WriteLine(uiMap.ToPrompt());
				Console.Write("\r\nSI > ");
				string cmd = Console.ReadLine().ToLower().Trim();
				var option = uiMap.GetOption(cmd);
				if(option != null)
					game.UserPortal.Choose(option);
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
				.Union( game.Spirit.InPlay )
				.Union( game.Spirit.DiscardPile )
				.Cast<PowerCard>()
//				.Union( game.Spirit.GetAvailableActions(Speed.Growth) )
				.Union( game.Spirit.GetAvailableActions(Phase.Fast) )
				.Union( game.Spirit.GetAvailableActions(Phase.Slow) )
				.Distinct()
//				.OrderBy( x => x.Speed == Speed.Growth ? 0 : x.Speed == Speed.Fast ? 1 : 2 )
				.OrderBy( x => x.Name )
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
			Console.WriteLine( $"\tEnergy: {game.Spirit.Energy}");
			Console.WriteLine( $"\tCards:  {game.Spirit.NumberOfCardsPlayablePerTurn}/turn" );
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
			Console.WriteLine( "\tDiscard: " + d.Discards.Count );
			Console.WriteLine( "\tRavage:  " + d.Ravage.Cards.Select(x=>x.Text).Join(" - " ));
			Console.WriteLine( "\tBuild:   " + d.Build.Cards.Select(x=>x.Text).Join(" - " ));
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
