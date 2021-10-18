using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class InciteTheMob {

		[SpiritCard("Incite the Mob",1, Element.Moon,Element.Fire,Element.Air ), Slow, FromPresence(1,Target.Invaders)]
		static public Task ActAsymc(TargetSpaceCtx _ ) { 
			// 1 invader with strife deals damage to other invaders (not to each)
			// 1 fear per invader this power destroyed.
			return Task.CompletedTask;
		}
	}
}
