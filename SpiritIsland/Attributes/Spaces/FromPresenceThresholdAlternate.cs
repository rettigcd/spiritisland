namespace SpiritIsland;

/// <summary>
/// Allows an alternate target based on meeting Element threshold
/// </summary>
public class FromPresenceThresholdAlternate : FromPresenceAttribute {

	readonly CountDictionary<Element> _threshold;
	readonly TargetCriteriaFactory _altTarget;

	public FromPresenceThresholdAlternate(int range, string target, string threshold, int altRange, string altTarget) 
		: base(range,target)
	{
		_threshold = ElementStrings.Parse(threshold);
		_altTarget = new TargetCriteriaFactory( altRange, altTarget );
	}

	protected override async Task<TargetCriteria> GetCriteria( SelfCtx ctx ) {
		return await ctx.Self.HasElements($"Target {_altTarget}",  _threshold)
			? _altTarget.Bind(ctx.Self)
			: await base.GetCriteria( ctx );
	}

}


