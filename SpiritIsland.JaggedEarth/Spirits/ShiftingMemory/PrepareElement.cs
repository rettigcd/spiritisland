namespace SpiritIsland.JaggedEarth;

public class PrepareElement( string context ) 
	: SpiritAction( "PrepareElement") 
{
	public override async Task ActAsync( Spirit self ) {
		if(self is ShiftingMemoryOfAges smoa) 
			await smoa.PrepareElement(context);
	}

}