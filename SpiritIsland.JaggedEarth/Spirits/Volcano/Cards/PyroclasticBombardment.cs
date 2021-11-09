using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class PyroclasticBombardment {
		[SpiritCard("Pyroclastic Bombardment", 3, Element.Fire, Element.Air, Element.Earth), Fast, FromSacredSite(2)]
		public static Task ActAsync(TargetSpaceCtx _ ) { 
			// 1 Damage to each town / city / dahan.
			// 1 Damage
			// 1 Damage to dahan.
			return Task.CompletedTask;
		}

	}

}
