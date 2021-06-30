using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;
using SpiritIsland.Core;

namespace SpiritIslandCmd {

	public class SelectPowerCards : IPhase {
		
		public string Prompt => uiMap.ToPrompt();

		readonly Spirit spirit;
		readonly Formatter formatter;
		List<PowerCard> selectedCards;
		List<PowerCard> options;
		int canPurchase;
		int energy;
		public UiMap uiMap {get; set;}
		public event Action Complete;

		public SelectPowerCards(Spirit spirit,Formatter formatter){
			this.spirit = spirit;
			this.formatter = formatter;
		}

		public void Initialize() {
			canPurchase = spirit.NumberOfCardsPlayablePerTurn;
			energy = spirit.Energy;
			selectedCards = new List<PowerCard>();
			EvaluateCards();
		}

		void EvaluateCards(){
			options = spirit.Hand
				.Except(selectedCards)
				.Where(c=>c.Cost<=energy && canPurchase>0)
				.ToList();
			if(options.Count == 0){
				spirit.BuyAvailableCards(selectedCards.ToArray());
				Complete?.Invoke();
				return;
			}
			uiMap = new UiMap( $"Buy power cards: (${energy} / {canPurchase})", GetDisplayOptions(), formatter );
		}

		List<IOption> GetDisplayOptions() {
			var options = new List<IOption>();
			options.AddRange( this.options );
			options.Add( new TextOption( "Done" ) );
			return options;
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
