namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
public class SpeedAttribute : Attribute, ISpeedBehavior {
	public SpeedAttribute(Phase speed ) { DisplaySpeed = speed; }
	public Phase DisplaySpeed { get; }

	public virtual Task<bool> IsActiveFor( Phase requestSpeed, Spirit _ ) {
		return Task.FromResult( DisplaySpeed.IsOneOf( requestSpeed, Phase.FastOrSlow ) );
	}
	public virtual bool CouldBeActiveFor( Phase requestSpeed, Spirit _ ) {
		return DisplaySpeed.IsOneOf( requestSpeed, Phase.FastOrSlow );
	}

}

public interface ISpeedBehavior {

	public Task<bool> IsActiveFor( Phase requestSpeed, Spirit spirit );

	public bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit );

}
