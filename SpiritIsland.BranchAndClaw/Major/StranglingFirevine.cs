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
			// !! won't find original if this was picked using a range-extender - would need to capture that info during the targetting process
			var originatingOptions = ctx.Adjacent
				.Where( a=> ctx.Self.Presence.Spaces.Contains(a) && a.IsSand )
				.ToArray();
			var originalCtx = await ctx.SelectSpace("Select origination space", originatingOptions);
			if(originalCtx != null)
				await originalCtx.Wilds.Add(1);

			// 1 damage per wilds in / adjacent to target land.
			int wildsDamage = ctx.Space.Range(1).Sum(s=>ctx.Target(s).Wilds.Count);

			// if you have 2 fire, 3 plant: // +1 damage per wilds in / adjacent to target land.
			if(await ctx.YouHave("2 fire,3 plant"))
				wildsDamage += wildsDamage;

			await ctx.DamageInvaders( wildsDamage );

			
		}


	}
}
