namespace SpiritIsland;

public class RangeExtender : ICalcRange {

	readonly int _extension;
	readonly ICalcRange _originalApi;

	static public void Extend( Spirit spirit, int extension ) {
		spirit.PowerRangeCalc = new RangeExtender( extension, spirit.PowerRangeCalc );
	}

	#region constructor
	public RangeExtender( int extension, ICalcRange originalApi ) {
		_extension = extension;
		_originalApi = originalApi;
	}
	#endregion

	public IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( IEnumerable<SpaceState> source, params TargetCriteria[] targetCriteria )
		=> _originalApi.GetTargetOptionsFromKnownSource( source, targetCriteria.ExtendRange(_extension) );

	#region private 

	#endregion
}

public static class TargetCriteriaExtensions {
	static public TargetCriteria[] ExtendRange( this TargetCriteria[] targetCriteria, int extension ) 
		=> targetCriteria
		.Select( tc => tc.ExtendRange( extension ) )
		.ToArray();
}