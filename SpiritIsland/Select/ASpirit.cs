namespace SpiritIsland.Select;

public class ASpirit : TypedDecision<Spirit> {

	public ASpirit( string powerName, IEnumerable<Spirit> spirits, Present present = Present.Always )
		: base( powerName+": Target Spirit", spirits, present ) {
	}

}