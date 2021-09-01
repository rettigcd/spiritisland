using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.BranchAndClaw {

	public class RavageEngineWithStrife : RavageEngine {

		public RavageEngineWithStrife(GameState gs,InvaderGroup group, ConfigureRavage cfg ) 
			: base( gs, group, cfg ) { }

		public override int GetDamageInflictedByInvaders() {
			// Calc damage
			int damageFromInvaders = Counts.Keys
				.Where(x=>!(x is StrifedInvader))
				.Select( invader => invader.FullHealth * Counts[invader] ).Sum();

			// decrement strife
			var strifed = Counts.Keys.OfType<StrifedInvader>()
				.OrderBy(x=>x.StrifeCount) // smallest first
				.ToArray();
			foreach(var orig in strifed) {
				var lessStrifed = orig.AddStrife(-1);
				Counts[lessStrifed] += Counts[orig];
				Counts[orig] = 0;
			}

			// Defend
			int damageInflictedFromInvaders = Math.Max( damageFromInvaders - cfg.Defend, 0 );

			return damageInflictedFromInvaders;
		}


	}


}
