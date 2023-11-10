namespace SpiritIsland.A;

public class Spirit : TypedDecision<SpiritIsland.Spirit> {

	public Spirit( string powerName, IEnumerable<SpiritIsland.Spirit> spirits, Present present = Present.Always )
		: base( powerName+": Target Spirit", spirits, present ) {
	}

}