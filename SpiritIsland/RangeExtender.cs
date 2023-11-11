namespace SpiritIsland;

public class RangeExtender : ICalcRange {

	static public void Extend( Spirit spirit, int extension ) {
		spirit.PowerRangeCalc = new RangeExtender( extension, spirit.PowerRangeCalc );
	}

	#region constructor
	public RangeExtender( int extension, ICalcRange originalApi ) {
		_extension = extension;
		_originalApi = originalApi;
	}
	#endregion

	public IEnumerable<SpaceState> GetSpaceOptions( IEnumerable<SpaceState> source, params TargetCriteria[] targetCriteria )
		=> _originalApi.GetSpaceOptions( source, targetCriteria.ExtendRange(_extension) );
	public IEnumerable<SpaceState> GetSpaceOptions( SpaceState source, TargetCriteria tc ) 
		=> _originalApi.GetSpaceOptions( source, tc.ExtendRange( _extension ) );

	#region private 
	readonly int _extension;
	readonly ICalcRange _originalApi;
	#endregion
}

public static class TargetCriteriaExtensions {
	static public TargetCriteria[] ExtendRange( this TargetCriteria[] targetCriteria, int extension ) 
		=> targetCriteria
		.Select( tc => tc.ExtendRange( extension ) )
		.ToArray();
}