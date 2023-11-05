namespace SpiritIsland.JaggedEarth;

public class PrepareElement : SpiritAction {

	readonly string context;
	public PrepareElement(string context ):base( "PrepareElement") { this.context = context; }

	public override async Task ActAsync( SelfCtx ctx ) {
		if(ctx.Self is ShiftingMemoryOfAges smoa) 
			await smoa.PrepareElement(context);
	}

}