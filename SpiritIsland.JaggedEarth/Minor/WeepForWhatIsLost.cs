namespace SpiritIsland.JaggedEarth;

public class WeepForWhatIsLost{ 
	[MinorCard("Weep for What Is Lost",0,Element.Fire,Element.Water,Element.Animal),Slow,FromPresence(1,Target.Blight)]
	static public Task ActAsync(TargetSpaceCtx ctx){
		// 1 fear per type of Invader present.
		ctx.AddFear( ctx.Tokens.InvaderTotal() );
		// Push up to 1 explorer / town per blight.
		return ctx.PushUpTo( ctx.Blight.Count, Invader.Explorer_Town );
	}
}