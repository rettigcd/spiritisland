using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class DomesticatedAnimalsGoBerserk{ 
		[MinorCard("Domesticated Animals Go Berserk",1,Element.Moon,Element.Fire,Element.Animal),Fast,FromPresence(0,Target.TownOrCity)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			// 1 fear.
			ctx.AddFear(1);

			// Defend 5.
			ctx.Defend(5);

			// If you have 3 moon:  Add 1 Beast
			if(await ctx.YouHave("3 moon"))
				await ctx.Beasts.Add(1);
		}
	}



}
