namespace SpiritIsland.NatureIncarnate;

public class ShadowTouchedRealm_RangeCalculator : DefaultRangeCalculator {

	static public SpecialRule Rule => new SpecialRule("Shadow-Touched Realm", "You land-targeting Powers can target Incarna as if it were a land ignoring Range.  EndlessDark is Inland and has no terrain.");

	public ShadowTouchedRealm_RangeCalculator() {}

	public override IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( IEnumerable<SpaceState> source, params TargetCriteria[] targetCriteria ) {
		return base.GetTargetOptionsFromKnownSource( source, targetCriteria )
			.Append(EndlessDark.Space.Tokens)
			.ToList();
	}
}
