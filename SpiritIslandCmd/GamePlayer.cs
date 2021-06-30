using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;
using SpiritIsland.Base;

namespace SpiritIslandCmd {

	public class GamePlayer {
		readonly IPhase selectGrowth;
		readonly IPhase resolveGrowth;
		readonly IPhase selectPowerCards;
		readonly IPhase fastActions;
		readonly IPhase invaders;
		readonly IPhase slowActions;
		readonly IPhase timePasses;

		readonly Spirit spirit;
		readonly GameState gameState;
		readonly Formatter formatter;
		readonly InvaderDeck invaderDeck;
		readonly Dictionary<string,Action> commandMap;

		IPhase phase;

		public GamePlayer(){
			spirit = new RiverSurges();
			Board board = Board.BuildBoardA();
			spirit.InitializePresence(board);
			gameState = new GameState(spirit){ Island = new Island(board)};
			gameState.InitBoards();
			invaderDeck = new InvaderDeck();
			gameState.Explore(invaderDeck.Explore);
			invaderDeck.Advance();

			formatter = new Formatter(spirit,gameState,invaderDeck);

			selectGrowth = new SelectGrowth(spirit,gameState,formatter);
			resolveGrowth = new ResolveActions(spirit,gameState,invaderDeck,Speed.Growth);
			selectPowerCards = new SelectPowerCards(spirit,formatter);
			fastActions = new ResolveActions(spirit,gameState,invaderDeck,Speed.Fast,true);
			invaders = new InvaderPhase(gameState,invaderDeck);
			slowActions = new ResolveActions(spirit,gameState,invaderDeck,Speed.Slow, true);
			timePasses = new TimePasses(spirit,gameState);

			selectGrowth.Complete += () => TransitionTo(resolveGrowth);
			resolveGrowth.Complete += () => TransitionTo(selectPowerCards);
			selectPowerCards.Complete += () => TransitionTo(fastActions);
			fastActions.Complete += () => TransitionTo(invaders);
			invaders.Complete += () => TransitionTo(slowActions);
			slowActions.Complete += () => TransitionTo(timePasses);
			timePasses.Complete += () => TransitionTo(selectGrowth);

			commandMap = new Dictionary<string, Action>{
				["spirit"] = ShowSpirit,
				["island"] = ShowIsland,
				["invaders"] = ShowInvaders,
				["cards"] = ShowCards,
				["?"] = ShowCommands,
				["q"] = Quit,
			};

		}

		void TransitionTo(IPhase phase){
			this.phase = phase; // ! this must go first! because .Initialize might trigger the next phase
			phase.Initialize();
		}

		public void Play(){

			TransitionTo( selectGrowth );

			while(phase != null){
				Console.WriteLine(phase.uiMap.ToPrompt());
				Console.Write("\r\nSI > ");
				string cmd = Console.ReadLine().ToLower();

				var option = phase.uiMap.GetOption(cmd);
				if(option != null)
					phase.Select(option);
				else
					Generic(cmd);
			}
		}

		bool Generic(string cmd){

			if( !commandMap.ContainsKey(cmd) ) return false;
			commandMap[cmd]();
			return true;
		}

		#region commands

		void Quit() {
			this.phase = null;
		}

		void ShowCards() {
			var cards = spirit.Hand
				.Union( spirit.PurchasedCards )
				.Union( spirit.DiscardPile )
				.Cast<IActionFactory>()
				.Union( spirit.UnresolvedActionFactories )
				.Distinct()
				.OrderBy( x => x.Speed == Speed.Growth ? 0 : x.Speed == Speed.Fast ? 1 : 2 )
				.ThenBy( x => x.Name )
				.ToList();
			int maxNameWidth = cards.Select( c => c.Name.Length ).Max();
			Console.WriteLine( "Cards:" );
			foreach(var card in cards)
				Console.WriteLine( "\t" + formatter.Format( card, maxNameWidth ) );
			Console.WriteLine();
		}

		void ShowSpirit() {
			Console.WriteLine( $"Spirit: {spirit.Text}" );
			// Growth Options
			// !!! Energy Track
			Console.WriteLine( $"\tEnergy: {spirit.EnergyPerTurn}/turn   Total:{spirit.Energy}");
			// !!! Card Track
			Console.WriteLine( $"\tCards:  {spirit.NumberOfCardsPlayablePerTurn}/turn" );
			// !!! Innate Powers
			// Special Rules
			Console.WriteLine();
		}

		void ShowIsland() {
			Console.WriteLine( "Island:" );
			foreach(var board in gameState.Island.Boards) {
				foreach(var space in board.Spaces)
					Console.WriteLine( "\t" + formatter.Format( space ) );
			}
			Console.WriteLine();
		}

		void ShowInvaders() {
			Console.WriteLine( "Invaders" );
			Console.WriteLine( "\tDiscard: " + invaderDeck.CountInDiscard );
			Console.WriteLine( "\tRavage:  " + invaderDeck.Ravage?.Text );
			Console.WriteLine( "\tBuild:   " + invaderDeck.Build?.Text );
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
		#endregion
	}

}
