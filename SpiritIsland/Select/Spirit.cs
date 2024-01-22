namespace SpiritIsland.A;

public class Spirit( string powerName, IEnumerable<SpiritIsland.Spirit> spirits, Present present = Present.Always ) 
	: TypedDecision<SpiritIsland.Spirit>( powerName+": Target Spirit", spirits, present ) 
{
}