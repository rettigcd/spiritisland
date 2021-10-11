using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PurifyingFlame {

		[MinorCard("Purifying Flame",1,Element.Sun,Element.Fire,Element.Air,Element.Plant)]
		[Slow]
		[FromSacredSite(1)]
		static public Task Act(TargetSpaceCtx ctx){

			int blightCount = ctx.BlightOnSpace;
			return ctx.SelectActionOption(
				new ActionOption($"{blightCount} damage", ()=>ctx.DamageInvaders(blightCount), blightCount>0 ),
				new ActionOption("Remove 1 blight", ()=>ctx.RemoveBlight(), blightCount>0 && ctx.IsOneOf( Terrain.Mountain, Terrain.Sand ) )
			);

		}

	}

}
