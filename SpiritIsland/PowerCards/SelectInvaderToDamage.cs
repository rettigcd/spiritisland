using System;
using System.Linq;

namespace SpiritIsland.PowerCards {
	public class SelectInvaderToDamage : IDecision {
		readonly InvaderGroup group;
		readonly int maxDamageAvailable;
		public SelectInvaderToDamage(InvaderGroup invaderGroup,int maxDamageAvailable){
			this.group = invaderGroup;
			this.maxDamageAvailable = maxDamageAvailable;
		}

		public string Prompt => $"Select invader to damage.";

		public IOption[] Options => group.InvaderTypesPresent.ToArray();

		public void Select( IOption option, ActionEngine engine ) {
			Invader invader = (Invader)option;

			// Calc Damage plan
			int maxDamageToThisInvader = Math.Min(invader.Health,maxDamageAvailable);
			var plan = new DamagePlan(group.Space,maxDamageToThisInvader,invader);
			// apply it to working-copy 
			group[plan.Invader]--;
			group[plan.DamagedInvader]++;
			engine.actions.Add(plan);

			// find recipient for remaining damage
			int remainingDamage = maxDamageAvailable - maxDamageToThisInvader;
			if(remainingDamage>0)
				engine.decisions.Push(new SelectInvaderToDamage(group,remainingDamage));
		}

	}

}