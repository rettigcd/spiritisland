using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.PowerCards {

	[PowerCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
	public class WashAway : IAction {

		public const string Name = "Wash Away";

		public WashAway(Spirit spirit,GameState gs){
			this.spirit = spirit;
			this.gameState = gs;

			var opt = GetOptions();
			while(opt.Length==1){
				InnerSelect(opt[0]);
				opt = GetOptions();
			}
		}

		public bool IsResolved => GetOptions().Length == 0;



		public void Apply() {
			if(target==null) return;

			// move 1 explorer
			gameState.RemoveExplorer(target);
			gameState.AddExplorer(invaderDestination);
		}

		public void Select(IOption option) {
			InnerSelect(option);
			var opt = GetOptions();
			while(opt.Length==1){
				InnerSelect(opt[0]);
				opt = GetOptions();
			}
		}

		public IOption[] GetOptions() {
			// select source
			if(target == null)
				return spirit.Presence
					.SelectMany(x=>x.SpacesWithin(1))
					.Distinct()
					.Where(s=>gameState.HasInvaders(s)) // !!! should only be explorers / towns
					.ToArray();

			if( remainingInvadersToPush > 0 ){

				if(invaderToPush==null)
					return targetGroup
						.InvaderTypesPresent
						// !!! Test that Cities don't show up
						.ToArray();

				return target.SpacesExactly(1).ToArray(); // !!! Will this return oceans also???
			}

			return new IOption[0];
		}

		void InnerSelect(IOption option) {
			if(target == null){
				target = (Space)option;
				int invadersInSpace = 1; // !!!gameState.GetInvaderSummary(1);
				remainingInvadersToPush = Math.Min(invadersInSpace,2);
				targetGroup = gameState.GetInvaderSummary(target);
				return;
			}

			if(remainingInvadersToPush>0){
				if(invaderToPush == null ){
					invaderToPush = (Invader)option;
					return;
				}

				if(invaderDestination == null){
					invaderDestination = (Space)option;
					--remainingInvadersToPush;
					return;
				}

			}

			throw new System.NotImplementedException();
		}


		readonly Spirit spirit;
		readonly GameState gameState;

		Space target;
		InvaderGroup targetGroup;

		Invader invaderToPush;
		Space invaderDestination;
		int remainingInvadersToPush;
	}
}
