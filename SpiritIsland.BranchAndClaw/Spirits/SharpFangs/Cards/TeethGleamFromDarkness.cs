using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class TeethGleamFromDarkness {

		[SpiritCard("Teeth Gleam from Darkness",1,Speed.Slow,Element.Moon,Element.Plant,Element.Animal)]
		[FromPresenceIn(1,Terrain.Jungle,Target.NoBlight)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			var beasts = ctx.Tokens.Beasts();
			await ctx.SelectActionOption(
				new ActionOption("1 fear, add 1 beast", ()=>{ ctx.AddFear(1); beasts.Count++; } ),
				new ActionOption("3 fear", ()=> ctx.AddFear(3), ctx.Tokens.HasInvaders() && beasts > 0 )
			);

		}

	}
}
