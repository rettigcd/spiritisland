using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIslandCmd {

	public class SelectPowerCards : IPhase {
		
		readonly Spirit spirit;
		List<PowerCard> selectedCards;
		List<PowerCard> options;
		int canPurchase;
		int energy;

		public SelectPowerCards(Spirit spirit){
			this.spirit = spirit;
		}

		public void Initialize() {
			canPurchase = spirit.NumberOfCardsPlayablePerTurn;
			energy = spirit.Energy;
			selectedCards = new List<PowerCard>();
			EvaluateCards();
		}

		void EvaluateCards(){
			options = spirit.AvailableCards
				.Except(selectedCards)
				.Where(c=>c.Cost<=energy && canPurchase>0)
				.ToList();
			if(options.Count == 0){
				spirit.BuyAvailableCards(selectedCards.ToArray());
				Complete?.Invoke();
				return;
			}
			UpdatePrompt();
		}

		void UpdatePrompt() {
			int i = 0;
			List<string> options = this.options
				.Select( c => $"\r\n{++i} : {c.Name} / {c.Cost} / {c.Speed}" )
				.ToList();
			options.Add( "\r\nD : done" );
			Prompt = $"Buy power cards: (${energy} / {canPurchase})" + options.Join( "" );
		}

		public string Prompt {get; private set;}

		public event Action Complete;

		public bool Handle( string cmd, int index ) {
			if(cmd=="d"){
				spirit.BuyAvailableCards(selectedCards.ToArray());
				Complete?.Invoke();
				return true;
			}
			if(index <0 || index>=options.Count) return false;

			var card = options[index];
			selectedCards.Add(card);

			energy -= card.Cost;
			--canPurchase;

			EvaluateCards();

			return true;
		}

	}

}
