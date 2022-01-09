using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class PyroclasticFlow {

		[MajorCard( "Pyroclastic Flow", 3, Element.Fire, Element.Air, Element.Earth )]
		[Fast]
		[FromPresenceIn( 1, Terrain.Mountain )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// 2 damage. Destroy all explorers
			await ctx.Invaders.Destroy(int.MaxValue,Invader.Explorer);
			int damage = 2;

			// if target land is J/W, add 1 blight
			if(ctx.IsOneOf(Terrain.Jungle,Terrain.Mountain))
				await ctx .AddBlight(1);

			// if you have 2 fire, 3 air, 2 earth: +4 damage. Add 1 wilds
			if(await ctx.YouHave("2 fire,3 air, 2 earth")) {
				await ctx.Wilds.Add(1);
				damage += 4;
			}
			await ctx.DamageInvaders( damage );
		}

	}
}
