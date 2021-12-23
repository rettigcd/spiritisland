using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class TerritorialStrife{ 
		[MinorCard("Territorial Strife",0,Element.Sun,Element.Fire,Element.Animal),Slow,FromPresence(1,Target.City)]
		static public Task ActAsync(TargetSpaceCtx ctx){
			return ctx.SelectActionOption( 
				Cmd.DamageToTownOrExplorer(3),
				Cmd.AddStrife(1)
			);
		}
	}
}
