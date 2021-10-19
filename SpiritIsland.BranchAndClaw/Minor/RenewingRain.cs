using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class RenewingRain {

		[MinorCard( "Renewing Rain", 1, Element.Water, Element.Earth, Element.Plant )]
		[Slow]
		[FromSacredSite( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			if(ctx.Space.Terrain.IsOneOf(Terrain.Jungle,Terrain.Sand)) // ??? should we be using Power Filters here?
				await ctx.RemoveBlight();

			if(ctx.YouHave("3 plant"))
				ctx.Wilds.Count++;

		}

	}

}
