using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;
using SpiritIsland.Core;

namespace SpiritIslandCmd {

	class SelectPowerCards : IPhase {

		public string Prompt => $"Buy power cards: (${energy} / {canPurchase})";

		public IOption[] Options { get {
			var options = new List<IOption>();
			options.AddRange( this.powerCardOptions );
			options.Add( new TextOption( "Done" ) );
			return options.ToArray();
		} }

		readonly Spirit spirit;
		List<PowerCard> selectedCards;
		List<PowerCard> powerCardOptions;
		int canPurchase;
		int energy;

		public event Action Complete;

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
			powerCardOptions = spirit.Hand
				.Except(selectedCards)
				.Where(c=>c.Cost<=energy && canPurchase>0)
				.ToList();
			if(powerCardOptions.Count == 0){
				spirit.BuyAvailableCards(selectedCards.ToArray());
				Complete?.Invoke();
				return;
			}
		}

		public void Select(IOption option){
			if(option is TextOption txt && txt.Text=="Done"){
				spirit.BuyAvailableCards(selectedCards.ToArray());
				Complete?.Invoke();
				return;
			}
			if(option is PowerCard card){
				selectedCards.Add(card);
				energy -= card.Cost;
				--canPurchase;
				EvaluateCards();
				return;
			}
		}

	}

}
