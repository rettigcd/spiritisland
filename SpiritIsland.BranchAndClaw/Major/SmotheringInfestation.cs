using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SmotheringInfestation {

		[MajorCard( "Smothering Infestation", 3, Element.Water, Element.Plant)]
		[Slow]
		[FromPresence( 0 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// add 1 disease
			ctx.Disease.Count++;

			// if you have 2 water and 2 plant:  1 dmaage to each invader (doing this first because our smart-damage result will be better)
			if(ctx.YouHave( "2 water,2 plant" ))
				await ctx.Invaders.ApplyDamageToEach( 1, Invader.Explorer, Invader.Town, Invader.City );

			// if target land is J/W, 2 fear and 3 damage
			if(ctx.Terrain.IsOneOf( Terrain.Jungle, Terrain.Wetland )) {
				ctx.AddFear(3);
				await ctx.DamageInvaders(3);
			}

		}

	}
}
