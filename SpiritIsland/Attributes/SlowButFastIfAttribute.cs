namespace SpiritIsland;

public class SlowButFastIfAttribute( string triggerElements ) : SpeedAttribute( Phase.Slow ) {

	readonly CountDictionary<Element> _triggerElements = ElementStrings.Parse( triggerElements );

	public override bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit ) {
		return base.CouldBeActiveFor( requestSpeed, spirit )
			|| requestSpeed == Phase.Fast && spirit.Elements.CouldHaveElements(_triggerElements) != ECouldHaveElements.No;
	}

}
