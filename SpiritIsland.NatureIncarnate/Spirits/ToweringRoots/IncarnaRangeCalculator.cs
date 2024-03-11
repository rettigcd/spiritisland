namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Extend Range+1 if source is Incarna
/// </summary>
public class IncarnaRangeCalculator( Spirit self ) : DefaultRangeCalculator {

	readonly Incarna _incarna = self.Incarna;

	public override IEnumerable<Space> GetSpaceOptions( Space source, TargetCriteria tc ) 
		=> base.GetSpaceOptions( source, source == _incarna.Space ? tc.ExtendRange(1) : tc );

}
