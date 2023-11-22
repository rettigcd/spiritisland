namespace SpiritIsland;

public class FastButSlowIfAttribute : SpeedAttribute {

	readonly CountDictionary<Element> _triggerElements;
	public FastButSlowIfAttribute(string triggerElements) : base( Phase.Fast ) { 
		_triggerElements = ElementStrings.Parse(triggerElements);
	}

	public override bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit ) {
		return base.CouldBeActiveFor( requestSpeed, spirit )
			|| requestSpeed == Phase.Slow && spirit.CouldHaveElements(_triggerElements);
	}


}