using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class TeethGleamFromDarkness {

		[SpiritCard("Teeth Gleam from Darkness",1,Speed.Slow,Element.Moon,Element.Plant,Element.Animal)]
		[FromPresenceIn(1,Terrain.Jungle,Target.NoBlight)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			bool do3fear = ctx.PowerInvaders.Counts.Keys.Any() 
				&& ctx.GameState.BAC().Beasts.AreOn(ctx.Target)
				&& await ctx.Self.UserSelectsFirstText("Select aciton","3 fear","1 fear + 1 beast");

			if(do3fear)
				ctx.AddFear(3);
			else {
				ctx.AddFear(1);
				ctx.GameState.BAC().Beasts.AddOneTo(ctx.Target);
			}
		}

	}
}
