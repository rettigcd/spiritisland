namespace SpiritIsland.JaggedEarth;

public class FlowDownriverBlowDownwind{ 

	[MinorCard("Flow Downriver, Blow Downwind",0,Element.Air,Element.Water,Element.Plant),Slow,FromSacredSite(2)]
	[Instructions( "Push up to 1 Blight / Explorer / Town." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync(TargetSpaceCtx ctx){
		// Push up to 1 blight, Explorer, Town.
		return ctx.PushUpTo(1, Human.Explorer_Town.Plus(Token.Blight));
	}
}