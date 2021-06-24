using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIslandCmd {

	public class SelectPowerCards : IPhase {
		
		readonly Spirit spirit;

		public SelectPowerCards(Spirit spirit){
			this.spirit = spirit;
		}

		public string Prompt {get; private set;}

		public event Action Complete;

		public bool Handle( string cmd ) {
			if(cmd=="d"){
				Complete?.Invoke();
				return true;
			}
			if(!int.TryParse(cmd,out int index)) return false;
			spirit.BuyAvailableCards(spirit.AvailableCards[index-1]);
			Initialize(); // hach
			return true;
		}



		public void Initialize() {
			int i=0;
			List<string> options = spirit.AvailableCards
				.Where(c=>c.Cost<=spirit.Energy)
				.Select(c=>$"\r\n{++i} : {c.Name} / {c.Cost} / {c.Speed}")
				.ToList();
			options.Add("\r\nD : done");
			Prompt = $"Buy power cards: (current energy:{spirit.Energy})" + options.Join("");
		}
	}

}
