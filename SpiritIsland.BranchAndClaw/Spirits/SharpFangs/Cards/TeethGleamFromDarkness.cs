using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class TeethGleamFromDarkness {

		[SpiritCard("Teeth Gleam from Darkness",1,Speed.Slow,Element.Moon,Element.Plant,Element.Animal)]
		[FromPresenceIn(1,Terrain.Jungle,Target.NoBlight)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			var beasts = ctx.Tokens.Beasts();
			bool do3fear = ctx.Tokens.HasInvaders() 
				&& beasts>0
				&& await ctx.Self.UserSelectsFirstText("Select aciton","3 fear","1 fear + 1 beast");

			if(do3fear)
				ctx.AddFear(3);
			else {
				ctx.AddFear(1);
				beasts.Count++;
			}
		}

	}
}
