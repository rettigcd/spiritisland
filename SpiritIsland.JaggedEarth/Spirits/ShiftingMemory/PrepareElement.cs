namespace SpiritIsland.JaggedEarth;

public class PrepareElement : GrowthActionFactory, IActionFactory {

	readonly string context;
	public PrepareElement(string context ) { this.context = context; }

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self is ShiftingMemoryOfAges smoa) 
			await smoa.PrepareElement(context);
	}

}