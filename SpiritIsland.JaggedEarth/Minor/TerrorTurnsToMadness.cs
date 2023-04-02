namespace SpiritIsland.JaggedEarth;

public class TerrorTurnsToMadness{ 

	[MinorCard("Terror Turns to Madness",0,Element.Moon,Element.Air,Element.Water),Slow,FromPresence(2,Target.Invaders)]
	[Instructions( "If the Terror Level is... Terror Level 1: 3 Fear. Terror Level 2: 2 Fear or add 1 Strife. Terror Lvl 3: Add 1 Strife." ), Artist( Artists.ShawnDaley )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		switch(ctx.GameState.Fear.TerrorLevel){
			case 1: ctx.AddFear(3); break;
			case 2: await ctx.SelectActionOption(Cmd.AddFear(2),Cmd.AddStrife(1)); break;
			case 3: await ctx.AddStrife(); break;
		}
	}

}
