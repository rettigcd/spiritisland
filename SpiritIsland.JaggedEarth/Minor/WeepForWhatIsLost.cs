namespace SpiritIsland.JaggedEarth;

public class WeepForWhatIsLost{ 

	[MinorCard("Weep for What is Lost",0,Element.Fire,Element.Water,Element.Animal),Slow,FromPresence(1,Filter.Blight)]
	[Instructions( "1 Fear per type of Invader present. Push up to 1 Explorer / Town per Blight." ), Artist( Artists.KatGuevara )]
	static public Task ActAsync(TargetSpaceCtx ctx){
		// 1 fear per type of Invader present.
		ctx.AddFear( ctx.Space.InvaderTotal() );
		// Push up to 1 explorer / town per blight.
		return ctx.PushUpTo( ctx.Blight.Count, Human.Explorer_Town );
	}
}