using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class StranglingFirevine {

		[MajorCard( "Strangling Firevine", 4, Element.Fire, Element.Plant )]
		[Slow]
		[FromPresenceIn( 1, Terrain.Sand )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// destory all explorers.
			await ctx.Invaders.Destroy(int.MaxValue,Invader.Explorer[1]);

			// Add 1 wilds.
			await ctx.Wilds.Add(1);

			// Add 1 wilds in the originating Sands.
			var original = ctx.Adjacent
				.Where( a=> ctx.Self.Presence.Spaces.Contains(a) && a.Terrain==Terrain.Sand )
				.FirstOrDefault(); // !! won't find original if this was picked using a range-extender - would need to capture that info during the targetting process

			// !!! let user pick which space was the originating sands

			if(original!=null)
				await ctx.Target(original).Wilds.Add(1);

			// 1 damage per wilds in / adjacent to target land.
			int wildsDamage = ctx.Space.Range(1).Sum(s=>ctx.Target(s).Wilds.Count);

			// if you have 2 fire, 3 plant: // +1 damage per wilds in / adjacent to target land.
			if(await ctx.YouHave("2 fire,3 plant"))
				wildsDamage += wildsDamage;

			await ctx.DamageInvaders( wildsDamage );

			
		}


	}
}
