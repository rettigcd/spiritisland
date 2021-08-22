using SpiritIsland.Basegame;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	static public class SpiritDecisionExtensinos {

		static public Task<Spirit> SelectSpirit(this Spirit spirit, Spirit[] spirits) {
			var result = new TaskCompletionSource<Spirit>();
			spirit.decisions.Push( new SelectAsync<Spirit>( "Select Spirit", spirits, Present.IfMoreThan1, result) );
			return result.Task;
		}

		static public Task<Space> SelectSpace( this Spirit spirit, string prompt, IEnumerable<Space> spaces, Present present = Present.IfMoreThan1 ) {
			var result = new TaskCompletionSource<Space>();
			spirit.decisions.Push( new SelectAsync<Space>( prompt, spaces.OrderBy(x=>x.Label).ToArray(), present, result ) );
			return result.Task;
		}

		static public Task<InvaderSpecific> SelectInvader( this Spirit spirit, string prompt, InvaderSpecific[] invaders, Present present = Present.IfMoreThan1 ) {
			var result = new TaskCompletionSource<InvaderSpecific>();

			var x = new SelectAsync<InvaderSpecific>(
				prompt,
				invaders,
				present,
				result
			);

			spirit.decisions.Push( x );
			return result.Task;
		}

		static public Task<IOption> SelectOption( this Spirit spirit, string prompt, IOption[] options, Present present = Present.IfMoreThan1 ) {
			var result = new TaskCompletionSource<IOption>();
			spirit.decisions.Push( new SelectAsync<IOption>(
				prompt,
				options,
				present,
				result
			) );
			return result.Task;
		}

		static public Task<IActionFactory> SelectFactory( this Spirit spirit, string prompt, IActionFactory[] options, Present present=Present.IfMoreThan1 ) {
			var result = new TaskCompletionSource<IActionFactory>();
			spirit.decisions.Push( new SelectAsync<IActionFactory>(
				prompt,
				options,
				present,
				result
			) );
			return result.Task;
		}

		static public async Task<string> SelectText( this Spirit spirit, string prompt, params string[] textOptions ) {
			TextOption[] options = textOptions.Select( x => new TextOption( x ) ).ToArray();
			var selection = await spirit.SelectTextOption( prompt, options );
			return selection?.Text;
		}
		static public Task<TextOption> SelectTextOption( this Spirit spirit, string prompt, params TextOption[] options ) {
			var result = new TaskCompletionSource<TextOption>();
			spirit.decisions.Push( new SelectAsync<TextOption>( prompt, options, Present.IfMoreThan1, result ) );
			return result.Task;
		}

		static public async Task<bool> UserSelectsFirstText( this Spirit spirit, string prompt, string option1, string option2 ) {
			return await spirit.SelectText( prompt, option1, option2 ) == option1;
		}

		static public async Task<int> SelectNumber( this Spirit spirit, string prompt, int max ) {
			List<string> numToMove = new List<string>();
			while(max > 0) numToMove.Add( (max--).ToString() );
			return int.Parse( await spirit.SelectText( prompt, numToMove.ToArray() ) );
		}

		//static public async Task<Element> SelectElement( this Spirit spirit, string prompt, IEnumerable<Element> options ) {
		//	var lookup = options.ToDictionary( el=>el.ToString(), el=>el );
		//	string text = await spirit.SelectText(prompt,lookup.Keys.ToArray());
		//	return lookup[text];
		//}

		static public async Task<Element> SelectElementAsync( this Spirit spirit, string prompt, IEnumerable<Element> elements ) {
			var selection = await spirit.SelectOption( prompt, elements.Select( x => new ItemOption<Element>( x ) ).ToArray(), Present.Always );
			return ((ItemOption<Element>)selection).Item;
		}


		#region relies on Spirit State

		static public Task<Track> SelectTrack( this Spirit spirit ) {
			var result = new TaskCompletionSource<Track>();

			spirit.decisions.Push( new SelectAsync<Track>(
				"Select Presence to place.",
				spirit.Presence.GetPlaceableFromTracks(), // state info, might someday be moved into game state, then this needs to move back to Action Engine
				Present.IfMoreThan1,
				result
			) );
			return result.Task;
		}

		static public async Task SelectActionsAndMakeFast( this Spirit spirit, int maxCountToMakeFast ) {

			IActionFactory[] CalcSlowFacts() => spirit
				.GetUnresolvedActionFactories( Speed.Slow )
				.ToArray();
			var slowFactories = CalcSlowFacts();
			// clip count to available slow stuff
			maxCountToMakeFast = System.Math.Min( maxCountToMakeFast, slowFactories.Length ); // !! unit test that we are limited by slow cards & by countToMakeFAst
			while(maxCountToMakeFast > 0) {
				var factory = await spirit.SelectFactory(
					$"Select action to make fast. max:{maxCountToMakeFast}",
					slowFactories,
					Present.Done
				);

				spirit.RemoveUnresolvedFactory( factory ); // remove it as slow
				spirit.AddActionFactory( new ChangeSpeed( factory, Speed.Fast ) ); // add as fast

				slowFactories = CalcSlowFacts();
				--maxCountToMakeFast;
			}
		}

		static public async Task SelectSpaceCardToReplayForCost( this Spirit spirit, int maxCost, List<SpaceTargetedArgs> played ) {
			maxCost = System.Math.Min( maxCost, spirit.Energy );
			var options = played.Select( p => p.Card ).ToArray();
			if(options.Length == 0) return;
			var factory = (TargetSpace_PowerCard)await spirit.SelectFactory( "Select card to replay", options );

			spirit.Energy -= factory.Cost;
			spirit.AddActionFactory( new ReplayOnSpace( factory, played.Single( p => p.Card == factory ).Target ) );
		}

		static public async Task ForgetPowerCard( this Spirit spirit ) {
			var options = spirit.PurchasedCards.Union( spirit.Hand ).Union( spirit.DiscardPile )
				.ToArray();
			var cardToForget = await spirit.SelectFactory( "Select power card to forget", options );
			spirit.Forget( (PowerCard)cardToForget );
		}

		#endregion

		static public Task ShowFearCardToUser( this Spirit spirit, string prompt, NamedFearCard cardToShow ) {
			return spirit.SelectOption( prompt, new IOption[] { new DisplayFearCard { Text = cardToShow.CardName } }, Present.Always );
		}


	}

	class ItemOption<T> : IOption {
		public T Item { get; }
		public ItemOption( T item ) { Item = item; }
		public string Text => Item.ToString();
	}

}