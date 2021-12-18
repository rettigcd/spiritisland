using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class DesiccatingWinds{ 
		[MinorCard("Desiccating Winds",1,Element.Fire,Element.Air,Element.Earth),Slow,FromSacredSite(1,Target.MountainOrSand)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			// If target land has badlands, 1 Damage.
			if(ctx.Badlands.Any)
				await ctx.DamageInvaders(1);
			// Add 1 badlands.
			await ctx.Badlands.Add(1);
		}
	}

}
