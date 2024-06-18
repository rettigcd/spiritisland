namespace SpiritIsland;

/// <summary>
/// Modifies FromPresence to allows an alternate target if spirit meets Element threshold
/// </summary>
public class FromPresenceThresholdAlternate( int range, string target, string threshold, int altRange, string altTarget ) 
	: FromPresenceAttribute(range,target)
{

	readonly CountDictionary<Element> _threshold = ElementStrings.Parse( threshold );
	readonly TargetCriteriaFactory _altTarget = new TargetCriteriaFactory( altRange, altTarget );

	protected override async Task<TargetCriteria> GetCriteria( Spirit self ) {
		return await self.HasElement( _threshold, $"Target {_altTarget}" )
			? _altTarget.Bind(self)
			: await base.GetCriteria( self );
	}

}


