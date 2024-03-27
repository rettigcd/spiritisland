namespace SpiritIsland;

public class FastButSlowIfAttribute( string triggerElements ) 
	: SpeedAttribute( Phase.Fast ) 
{

	readonly CountDictionary<Element> _triggerElements = ElementStrings.Parse( triggerElements );

	public override bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit ) {
		return base.CouldBeActiveFor( requestSpeed, spirit )
			|| requestSpeed == Phase.Slow && spirit.CouldHaveElements(_triggerElements) != ECouldHaveElements.No;
	}

}