using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class ThrivingChokefungus{ 
		[MinorCard("Thriving Chokefungus",1,Element.Moon,Element.Water,Element.Plant),Slow,FromSacredSite(1,Target.JungleOrWetland)]
		static public Task ActAsync(TargetSpaceCtx ctx){
			// add 1 disease
			ctx.Disease.Add(1);

			// add 1 badlands
			ctx.Badlands.Add(1);

			return Task.CompletedTask;
		}
	}



}
