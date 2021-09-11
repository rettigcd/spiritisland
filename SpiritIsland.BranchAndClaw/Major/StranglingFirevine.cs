using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class StranglingFirevine {

		[MajorCard( "Strangling Firevine", 4, Speed.Slow, Element.Fire, Element.Plant )]
		[FromPresenceIn( 1, Terrain.Sand )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// destory all explorers.
			await ctx.Invaders.Destroy(int.MaxValue,Invader.Explorer[1]);

			// Add 1 wilds.
			ctx.Tokens.Wilds().Count++;

			// Add 1 wilds in the originating Sands.
			var original = ctx.Adjacents
				.Where( a=> ctx.Self.SacredSites.Contains(a) && a.Terrain==Terrain.Sand )
				.FirstOrDefault(); // !! won't find original if this was picked using a range-extender - would need to capture that info during the targetting process
			if(original!=null)
				ctx.TargetSpace(original).Tokens.Wilds().Count++;

			// 1 damage per wilds in / adjacent to target land.
			int wildsDamage = ctx.Space.Range(1).Sum(s=>ctx.TargetSpace(s).Tokens.Wilds().Count);

			// if you have 2 fire, 3 plant: // +1 damage per wilds in / adjacent to target land.
			if(ctx.YouHave( "2 fire,3 plant" ))
				wildsDamage += wildsDamage;

			await ctx.DamageInvaders( wildsDamage );

			
		}


	}
}
