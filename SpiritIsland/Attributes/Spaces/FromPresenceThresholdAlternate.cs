namespace SpiritIsland;

/// <summary>
/// Modifies FromPresence to allows an alternate target if spirit meets Element threshold
/// </summary>
/// <param name="range">The standard Range</param>
/// <param name="target">The standard Target</param>
/// <param name="threshold">Element threshold that triggers alternate range/target</param>
/// <param name="altRange">The range in effect if Elemental threshold reached.</param>
/// <param name="altTarget">The Target in effect if Elemental threshold reached.</param>
public class FromPresenceThresholdAlternate( int range, string target, string threshold, int altRange, string altTarget ) 
	: FromPresenceAttribute(range,target)
{

	readonly CountDictionary<Element> _threshold = ElementStrings.Parse( threshold );
	readonly TargetCriteriaFactory _altTarget = new TargetCriteriaFactory( altRange, altTarget );

	protected override async Task<TargetCriteria> ApplySpiritModsToGetTargetCriteria( Spirit self ) {
		return await self.Elements.AskToMeetThreshold( _threshold, $"Target {_altTarget}" )
			? _altTarget.Bind(self)
			: await base.ApplySpiritModsToGetTargetCriteria( self );
	}

}


