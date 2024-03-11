namespace SpiritIsland;

public class RangeExtender( int extension, ICalcRange originalApi ) : ICalcRange {

	static public void Extend( Spirit spirit, int extension ) {
		spirit.PowerRangeCalc = new RangeExtender( extension, spirit.PowerRangeCalc );
	}

	#region constructor
	#endregion

	public IEnumerable<Space> GetSpaceOptions( IEnumerable<Space> source, params TargetCriteria[] targetCriteria )
		=> _originalApi.GetSpaceOptions( source, targetCriteria.ExtendRange(_extension) );
	public IEnumerable<Space> GetSpaceOptions( Space source, TargetCriteria tc ) 
		=> _originalApi.GetSpaceOptions( source, tc.ExtendRange( _extension ) );

	#region private 
	readonly int _extension = extension;
	readonly ICalcRange _originalApi = originalApi;
	#endregion
}

public static class TargetCriteriaExtensions {
	static public TargetCriteria[] ExtendRange( this TargetCriteria[] targetCriteria, int extension ) 
		=> targetCriteria
		.Select( tc => tc.ExtendRange( extension ) )
		.ToArray();
}