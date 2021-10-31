using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.ShiftingMemory {
	public class ShareSecretsOfSurvival {

		[SpiritCard("Share Secrets of Survival",0,Element.Sun,Element.Air,Element.Earth),Fast,FromSacredSite(1)]
		static public Task ActAsync(TargetSpaceCtx ctx ) { 
			// Each time Dahan would be Destoryed in target land, Destroy 2 fewer dahan.
			// OR
			// Gather up to 2 Dahan.
			return Task.CompletedTask;
		}

	}

}
