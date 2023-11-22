namespace SpiritIsland;

public class SlowButFastIfAttribute : SpeedAttribute {

	readonly CountDictionary<Element> _triggerElements;
	public SlowButFastIfAttribute(string triggerElements) : base( Phase.Slow ) { 
		_triggerElements = ElementStrings.Parse( triggerElements );
	}

	public override bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit ) {
		return base.CouldBeActiveFor( requestSpeed, spirit )
			|| requestSpeed == Phase.Fast && spirit.CouldHaveElements(_triggerElements);
	}

}
