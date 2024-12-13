namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Extend Range+1 if source is Incarna
/// </summary>
public class IncarnaRangeCalculator( Spirit self ) : DefaultRangeCalculator {

	readonly Incarna _incarna = self.Incarna;

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc)
		=> base.GetTargetingRoute(source, source == _incarna.Space ? tc.ExtendRange(1) : tc);
	
}
