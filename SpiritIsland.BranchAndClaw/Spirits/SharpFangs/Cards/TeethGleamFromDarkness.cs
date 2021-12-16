using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class TeethGleamFromDarkness {

		[SpiritCard("Teeth Gleam from Darkness",1,Element.Moon,Element.Plant,Element.Animal)]
		[Slow]
		[FromPresenceIn(1,Terrain.Jungle,Target.NoBlight)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			await ctx.SelectActionOption(
				new SpaceAction("1 fear, add 1 beast", ctx => { ctx.AddFear(1); ctx.Beasts.Add(1); } ),
				new SpaceAction("3 fear", ctx => ctx.AddFear(3) )
					.Cond( ctx.Tokens.HasInvaders() && ctx.Beasts.Any )
			);

		}

	}
}
