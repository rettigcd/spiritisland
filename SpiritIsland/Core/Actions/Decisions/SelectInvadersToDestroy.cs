using System;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {
	class SelectInvadersToDestroy : IDecision {

		readonly InvaderGroup grp;
		readonly int remaining; 
		readonly string[] labels;
		readonly ActionEngine engine;

		public SelectInvadersToDestroy(ActionEngine engine, InvaderGroup grp, int count, params string[] labels){
			this.engine = engine;
			remaining = count-1;
			this.grp = grp;
			this.labels = labels;

		}

		public string Prompt => "Select invader to destroy.";

		public IOption[] Options => grp.InvaderTypesPresent
			.Where(i=>labels.Contains(i.Summary))
			.Distinct()
			.Cast<IOption>()
			.ToArray();

		public void Select( IOption option ) {
			Invader invader = (Invader)option;
			grp.ApplyDamage(new DamagePlan(invader.Health,invader));
			if(remaining>0)
				engine.decisions.Push(new SelectInvadersToDestroy(engine,grp,remaining,labels));
		}
	}

}
