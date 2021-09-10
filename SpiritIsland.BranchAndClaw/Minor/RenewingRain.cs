using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class RenewingRain {

		[MinorCard( "Renewing Rain", 1, Speed.Slow, Element.Water, Element.Earth, Element.Plant )]
		[FromSacredSite( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			if(ctx.Space.Terrain.IsOneOf(Terrain.Jungle,Terrain.Sand)) // ??? should we be using Power Filters here?
				ctx.RemoveBlight();

			if(ctx.YouHave("3 plant"))
				ctx.Tokens.Wilds().Count++;

			return Task.CompletedTask;
		}

	}

}
