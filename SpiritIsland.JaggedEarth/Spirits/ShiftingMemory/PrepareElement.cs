namespace SpiritIsland.JaggedEarth;

public class PrepareElement : SpiritAction {

	readonly string context;
	public PrepareElement(string context ):base( "PrepareElement") { this.context = context; }

	public override async Task ActAsync( Spirit self ) {
		if(self is ShiftingMemoryOfAges smoa) 
			await smoa.PrepareElement(context);
	}

}