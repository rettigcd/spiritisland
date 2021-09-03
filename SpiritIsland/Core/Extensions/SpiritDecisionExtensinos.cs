using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	static public class SpiritDecisionExtensinos {

		static public Task<T> Select<T>( this Spirit spirit, string prompt, T[] options, Present present = Present.IfMoreThan1 ) where T : class, IOption {
			return spirit.Action.Decide( new TypedDecision<T>( prompt, options, present ) );
		}

		#region Simple Wrappers

		// wrapper - switches type to String
		static public async Task<string> SelectText( this Spirit spirit, string prompt, params string[] textOptions ) {
			TextOption[] options = textOptions.Select( x => new TextOption( x ) ).ToArray();
			var selection = await spirit.Select( prompt, options );
			return selection?.Text;
		}

		// wrapper - switches type to Element
		static public async Task<Element> SelectElementAsync( this Spirit spirit, string prompt, IEnumerable<Element> elements ) {
			var selection = await spirit.Select( prompt, elements.Select( x => new ItemOption<Element>( x ) ).ToArray(), Present.Always );
			return ((ItemOption<Element>)selection).Item;
		}

		// wrapper - checks for first response
		static public async Task<bool> UserSelectsFirstText( this Spirit spirit, string prompt, string option1, string option2 ) {
			return await spirit.SelectText( prompt, option1, option2 ) == option1;
		}

		// wrapper
		static public async Task<int> SelectNumber( this Spirit spirit, string prompt, int max ) {
			List<string> numToMove = new List<string>();
			while(max > 0) numToMove.Add( (max--).ToString() );
			return int.Parse( await spirit.SelectText( prompt, numToMove.ToArray() ) );
		}

		#endregion

		#region Higher Level of abstraction / uses Spirit State

		static public Task<Track> SelectTrack( this Spirit spirit ) {
			return spirit.Action.Decide( new TypedDecision<Track>(
				"Select Presence to place.",
				spirit.Presence.GetPlaceableFromTracks(), // state info, might someday be moved into game state, then this needs to move back to Action Engine
				Present.IfMoreThan1
			) );
		}

		static public async Task SelectActionsAndMakeFast( this Spirit spirit, GameState gameState, int maxCountToMakeFast ) {

			IActionFactory[] CalcSlowFacts() => spirit
				.GetAvailableActions( Speed.Slow )
				.ToArray();
			IActionFactory[] slowFactories = CalcSlowFacts();
			// clip count to available slow stuff
			maxCountToMakeFast = System.Math.Min( maxCountToMakeFast, slowFactories.Length ); // !! unit test that we are limited by slow cards & by countToMakeFAst

			while(maxCountToMakeFast > 0) {
				var factory = await spirit.Select(
					$"Select action to make fast. max:{maxCountToMakeFast}",
					slowFactories,
					Present.Done
				);
				if(factory==null)
					break;

				var speedReseter = new RememberFactorySpeed(factory);
				factory.Speed = Speed.Fast;

				gameState.TimePasses_ThisRound.Push( speedReseter.Reset );

				slowFactories = CalcSlowFacts();
				--maxCountToMakeFast;
			}
		}

		/// <summary>
		/// This provides a javascript-like closure to capture the factory that needs reset to fast;
		/// </summary>
		class RememberFactorySpeed {
			readonly IActionFactory factory;
			readonly Speed originalSpeed;
			public RememberFactorySpeed( IActionFactory factory ) {
				this.factory = factory;
				this.originalSpeed = factory.Speed;
			}
			public Task Reset(GameState _) {
				factory.Speed = originalSpeed;
				return Task.CompletedTask;
			}
		}

		static public async Task SelectCardToReplayForCost( this Spirit spirit, int maxCost, PowerCard[] options ) {
			maxCost = System.Math.Min( maxCost, spirit.Energy );
			if(options.Length == 0) return;
			var factory = (TargetSpace_PowerCard)await spirit.Select( "Select card to replay", options.Where(x=>x.Cost<=maxCost).ToArray() );

			spirit.Energy -= factory.Cost;
			spirit.AddActionFactory( factory );
		}

		static public async Task ForgetPowerCard( this Spirit spirit ) {
			var options = spirit.PurchasedCards.Union( spirit.Hand ).Union( spirit.DiscardPile )
				.ToArray();
			var cardToForget = await spirit.Select( "Select power card to forget", options );
			spirit.Forget( (PowerCard)cardToForget );
		}

		#endregion

		static public Task ShowFearCardToUser( this Spirit spirit, string prompt, NamedFearCard cardToShow ) {
			return spirit.Select( prompt, new IOption[] { new DisplayFearCard { Text = cardToShow.CardName } }, Present.Always );
		}

	}

	class ItemOption<T> : IOption {
		public T Item { get; }
		public ItemOption( T item ) { Item = item; }
		public string Text => Item.ToString();
	}

}