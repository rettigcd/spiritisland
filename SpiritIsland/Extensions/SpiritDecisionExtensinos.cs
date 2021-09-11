﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	static public class SpiritDecisionExtensinos {

		// !!! used almost entirely to select power cards - create a separte Select_PowerCard(...) 
		static public Task<T> Select<T>( this Spirit spirit, string prompt, T[] options, Present present = Present.IfMoreThan1 ) where T : class, IOption {
			return spirit.Action.Decision( new Decision.TypedDecision<T>( prompt, options, present ) );
		}

		#region Simple Wrappers

		// wrapper - switches type to String
		static public async Task<string> SelectText( this Spirit spirit, string prompt, string[] textOptions, Present present=Present.IfMoreThan1 ) {
			TextOption[] options = textOptions.Select( x => new TextOption( x ) ).ToArray();
			var selection = await spirit.Select( prompt, options, present );
			return selection?.Text;
		}

		// wrapper - switches type to Element
		static async Task<Element> SelectElement( this Spirit spirit, string prompt, IEnumerable<Element> elements ) {
			var selection = await spirit.Select( prompt, elements.Select( x => new ItemOption<Element>( x ) ).ToArray(), Present.Always );
			return ((ItemOption<Element>)selection).Item;
		}

		static public async Task<Element[]> SelectElements( this Spirit spirit, int totalToGain, params Element[] elements ) {
			var selected = new List<Element>();
			List<Element> available = elements.ToList();

			while(selected.Count < totalToGain) {
				var el = await spirit.SelectElement( $"Select {selected.Count + 1} of {totalToGain} element to gain", available );
				selected.Add( el );
				available.Remove( el );
			}
			return selected.ToArray();
		}


		// only used for Major/Minor deck selection and presenting erors / Fear card.
		static public async Task<bool> UserSelectsFirstText( this Spirit spirit, string prompt, params string[] options ) {
			return await spirit.SelectText( prompt, options ) == options[0];
		}

		// wrapper
		static public async Task<int> SelectNumber( this Spirit spirit, string prompt, int max, int min = 1 ) {
			List<string> numToMove = new List<string>();
			int cur = max;
			while(min <= cur ) numToMove.Add( (cur--).ToString() );
			return int.Parse( await spirit.SelectText( prompt, numToMove.ToArray() ) );
		}

		#endregion

		#region Higher Level of abstraction / uses Spirit State

		static public Task<Track> SelectTrack( this Spirit spirit ) {
			return spirit.Action.Decision( new Decision.PresenceToRemoveFromTrack(spirit) );
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