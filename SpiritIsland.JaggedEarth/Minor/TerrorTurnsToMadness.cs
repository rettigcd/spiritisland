using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class TerrorTurnsToMadness{ 
		[MinorCard("Terror Turns to Madness",0,Element.Moon,Element.Air,Element.Water),Slow,FromPresence(2,Target.Invaders)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			switch(ctx.GameState.Fear.TerrorLevel){
				case 1: ctx.AddFear(3); break;
				case 2: await ctx.SelectActionOption(Cmd.AddFear(2),Cmd.Add1Strife); break;
				case 3: await ctx.AddStrife(); break;
			}
		}
	}
}
