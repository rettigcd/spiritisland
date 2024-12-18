#nullable enable
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
	/// <param name="filterOptions">If filterOptions not empty, Space must match at least 1 of the Filters.</param>
	public TargetCriteria( int range, Spirit? self, params string[] filterOptions ) :base(self,filterOptions) {
		Range = range;
	}

	#endregion

	/// <summary> Is not used for Matches/criteria. </summary>
	public int Range { get; }

	// Virtual so OfferPassageBetweenWorlds can do multiple criteria
	public virtual TargetCriteria ExtendRange( int extension ) => new TargetCriteria( Range + extension, _self, _filters );

}

/// <summary>
/// Late-binds to Spirit so we can define TargetCriteria without the spirit
/// </summary>
public class TargetCriteriaFactory {
	/// <summary> For no-filter criteria </summary>
	public TargetCriteriaFactory( int range ) : base() {
		_range = range;
	}

	/// <summary> For early binding Spirit dependent criteria </summary>
	public TargetCriteriaFactory( int range, params string[] filters ){
		_range = range;
		_filters = filters;
	}

	public TargetCriteria Bind(Spirit spirit) => _filters is null 
		? new TargetCriteria(_range) 
		: new TargetCriteria(_range,spirit,_filters);

	readonly int _range;
	readonly string[]? _filters;
}