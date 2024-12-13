namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Extend Range+1 if source is Incarna
/// </summary>
public class HeartTree( Spirit self ) : DefaultRangeCalculator {

	static public SpecialRule Rule => new SpecialRule(
		"Heart-Tree Guards the Land", 
		"You have an Incarna. Your Powers get +1 range if Incarna is in the origin land.  Invaders/Dahan/Beast can't be damaged or destroyed at Incarna.  Empower Encarna the first time it's in a land with 3 or more vitality.  Skip all Build Actions at empowered Incarna."
	);

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc)
		=> base.GetTargetingRoute(source, source == _incarna.Space ? tc.ExtendRange(1) : tc);

	readonly Incarna _incarna = self.Incarna;

}
