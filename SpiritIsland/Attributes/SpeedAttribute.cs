namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
public class SpeedAttribute( Phase speed ) : Attribute, ISpeedBehavior {
	public Phase DisplaySpeed { get; } = speed;
	public virtual bool CouldBeActiveFor( Phase requestSpeed, Spirit _ ) {
		return DisplaySpeed.IsOneOf( requestSpeed, Phase.FastOrSlow );
	}

}

public interface ISpeedBehavior {

	public bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit );

}
