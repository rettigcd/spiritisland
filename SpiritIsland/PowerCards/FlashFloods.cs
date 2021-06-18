using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.PowerCards {

	[PowerCard(FlashFloods.Name,2,Speed.Fast,Element.Sun,Element.Water)]
	public class FlashFloods : IAction {
		// Target: range 1 (any)
		// +1 damage, if costal +1 additional damage

		public const string Name = "Flash Floods";

		public FlashFloods(Spirit spirit,GameState gameState){
			this.spirit = spirit;
			this.gameState = gameState;
		}

		public bool IsResolved => targetSpace != null && damagePlan != null;

		public IOption[] GetOptions(){
			if(targetSpace == null)
				return spirit.Presence
					.SelectMany(p=>p.SpacesWithin(1))
					.Where(x=>x.IsLand)
					.Distinct()
					.ToArray();

			if(damagePlan == null)
				return CalcDamageOptions();

			return new IOption[0]; // ???
		}

		DamagePlan[] CalcDamageOptions() {

			// !!! ignores already damaged Cities / Towns

			var options = new List<DamagePlan>();
			var invaderSummary = gameState.GetInvaderGroup(targetSpace);

			int damage = targetSpace.IsCostal ? 2 : 1;
			while(damage>0){
				if( damage<=3 && invaderSummary.HasCity ){
					options.Add(new DamagePlan(damage, Invader.City));
				}
				if( damage<=2 && invaderSummary.HasTown ){
					options.Add(new DamagePlan( damage, Invader.Town));
				}
				if( damage<=1 && invaderSummary.HasExplorer ){
					options.Add(new DamagePlan( damage, Invader.Explorer));
				}
				--damage;
			}
			return options.ToArray();
		}

		public void Apply() {
			gameState.ApplyDamage(targetSpace,damagePlan);
		}

		public void Select(IOption option) {
			if(targetSpace == null) {
				targetSpace = (Space)option;
				return;
			}
			
			if(damagePlan == null) {
				damagePlan = (DamagePlan)option;
				return;
			}

			throw new NotImplementedException(); // ???
		}

		readonly GameState gameState;
		readonly Spirit spirit;
		Space targetSpace;
		DamagePlan damagePlan;

	}

}