namespace SpiritIsland;

/// <summary>
/// Provides 2 services:
///		1) Nominal range from source
///		2) Binding to a Spirit returns a Predicate for matching spaces.
/// </summary>
public class TargetCriteria : SpaceCriteria {

	#region constructor

	/// <summary> For no-filter criteria </summary>
	public TargetCriteria( int range ):base() {
		Range = range;
	}

	/// <summary> For early binding Spirit dependent criteria </summary>
	public TargetCriteria( int range, Spirit self, params string[] filters ) :base(self,filters) {
		Range = range;
	}

	#endregion

	/// <summary> Is not used for Matches/criteria. </summary>
	public int Range { get; }

	// Virtual so OfferPassageBetweenWorlds can do multiple criteria
	public virtual TargetCriteria ExtendRange( int extension ) => new TargetCriteria( Range + extension, _self, _filters );

}
