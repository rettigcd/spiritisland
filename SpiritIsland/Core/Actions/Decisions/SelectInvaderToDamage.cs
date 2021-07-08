using System;
using System.Linq;

namespace SpiritIsland.Core {

	public class SelectInvaderToDamage : IDecision {
		readonly InvaderGroup group;
		readonly int maxDamageAvailable;
		readonly ActionEngine engine;
		public SelectInvaderToDamage(ActionEngine engine, InvaderGroup invaderGroup,int maxDamageAvailable){
			this.engine = engine;
			this.group = invaderGroup;
			this.maxDamageAvailable = maxDamageAvailable;
		}

		public string Prompt => $"Select invader to damage.";

		public IOption[] Options => group.InvaderTypesPresent.ToArray();

		public void Select( IOption option ) {
			Invader invader = (Invader)option;

			// Calc Damage plan
			int maxDamageToThisInvader = Math.Min(invader.Health,maxDamageAvailable);
			var plan = new DamagePlanAction(group.Space,maxDamageToThisInvader,invader);
			// apply it to working-copy 
			group[plan.Invader]--;
			group[plan.DamagedInvader]++;
			engine.actions.Add(plan);

			// find recipient for remaining damage
			int remainingDamage = maxDamageAvailable - maxDamageToThisInvader;
			if(remainingDamage>0)
				engine.decisions.Push(new SelectInvaderToDamage(engine,group,remainingDamage));
		}

	}

}