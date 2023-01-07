namespace SpiritIsland.JaggedEarth;

public class FlowDownriverBlowDownwind{ 
	[MinorCard("Flow Downriver Blow Downwind",0,Element.Air,Element.Water,Element.Plant),Slow,FromSacredSite(2)]
	static public Task ActAsync(TargetSpaceCtx ctx){
		// Push up to 1 blight, Explorer, Town.
		return ctx.PushUpTo(1, Invader.Explorer_Town.Plus(TokenType.Blight));
	}
}