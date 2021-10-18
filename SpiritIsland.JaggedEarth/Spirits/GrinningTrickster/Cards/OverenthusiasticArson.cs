using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class OverenthusiasticArson {
		[SpiritCard("Overenthusiastic Arson",1,Element.Fire,Element.Air), Fast, FromPresence(1)]
		static public Task ActAsymc(TargetSpaceCtx _ ) { 
			// Destory 1 town
			// discard the top card of the minor power deck.  IF it provides fire: 1 fear, 2 damage, and add 1 blight
			return Task.CompletedTask;
		}
	}
}
