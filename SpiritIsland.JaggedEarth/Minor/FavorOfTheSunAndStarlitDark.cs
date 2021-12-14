using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class FavorOfTheSunAndStarlitDark{ 
		[MinorCard("Favor of the Sun and Starlit Dark",1,Element.Sun,Element.Moon,Element.Plant),Fast,FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			// Defend 4
			ctx.Defend(4);

			// Push up to 1 blight.
			await ctx.PushUpTo(4,TokenType.Blight.Generic);

			// If you have 2 sun: 1 fear
			if(await ctx.YouHave("2 sun"))
				ctx.AddFear(1);
		}
	}

}
