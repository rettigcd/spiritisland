using System;
using System.Linq;

namespace SpiritIsland {

	public class RavageEngineWithStrife : RavageEngine {

		public RavageEngineWithStrife(GameState gs,InvaderGroup group, ConfigureRavage cfg ) 
			: base( gs, group, cfg ) { }

		public override int GetDamageInflictedByInvaders() {
			// Calc damage
			int damageFromInvaders = Counts.Invaders()
				.Where(x=>!(x is StrifedInvader))
				.Select( invader => invader.FullHealth * Counts[invader] ).Sum();

			// decrement strife
			var strifed = Counts.Invaders().OfType<StrifedInvader>()
				.OrderBy(x=>x.StrifeCount) // smallest first
				.ToArray();
			foreach(var orig in strifed) {
				var lessStrifed = orig.AddStrife(-1);
				Counts[lessStrifed] += Counts[orig];
				Counts[orig] = 0;
			}

			// Defend
			int damageInflictedFromInvaders = Math.Max( damageFromInvaders - Defend, 0 );

			return damageInflictedFromInvaders;
		}


	}


}
