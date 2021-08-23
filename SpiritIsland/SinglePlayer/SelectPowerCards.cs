using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.SinglePlayer {

	class SelectPowerCards : IPhase {

		public IDecision Current { get; private set; }

		public string Prompt => Current.Prompt;

		public IOption[] Options => Current.Options;

		readonly Spirit spirit;
		List<PowerCard> selectedCards;
		List<PowerCard> powerCardOptions;
		int canPurchase;
		int energy;

		public event Action Complete;

		public bool AllowAutoSelect { get; set; } = true;

		public SelectPowerCards( Spirit spirit ) {
			this.spirit = spirit;
		}

		public void Initialize() {
			canPurchase = spirit.NumberOfCardsPlayablePerTurn;
			energy = spirit.Energy;
			selectedCards = new List<PowerCard>();

			EvaluateCards();
		}

		void EvaluateCards() {
			powerCardOptions = spirit.Hand
				.Except( selectedCards )
				.Where( c => c.Cost <= energy && canPurchase > 0 )
				.ToList();
			Current = new Decision {
				Prompt = $"Buy power cards: (${energy} / {canPurchase})",
				Options = CalcOptions( this.powerCardOptions ),
			};

			if(powerCardOptions.Count == 0) {
				spirit.PurchaseAvailableCards( selectedCards.ToArray() );
				Complete?.Invoke();
				return;
			}
		}

		public void Select( IOption option ) {
			if(TextOption.Done.Matches( option )) {
				spirit.PurchaseAvailableCards( selectedCards.ToArray() );
				Complete?.Invoke();
				return;
			}
			if(option is PowerCard card) {
				selectedCards.Add( card );
				energy -= card.Cost;
				--canPurchase;
				EvaluateCards();
				return;
			}
		}

		static IOption[] CalcOptions( List<PowerCard> xx ) {
			var options = new List<IOption>();
			options.AddRange( xx );
			options.Add( TextOption.Done );
			return options.ToArray();
		}

	}

}
