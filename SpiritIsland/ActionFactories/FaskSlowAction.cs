namespace SpiritIsland;

/// <summary>
/// Wraps a SelfCmd turning it into a GrowthAction
/// </summary>
public class FastSlowAction(IActOn<Spirit> cmd) : IActionFactory {

	public readonly IActOn<Spirit> Cmd = cmd;

	string IOption.Text => Cmd.Description;

	#region IActionFactory
	string IActionFactory.Title => Cmd.Description;
	Task IActionFactory.ActivateAsync(Spirit self) => Cmd.ActAsync(self);
	bool IActionFactory.CouldActivateDuring(Phase speed, Spirit _) => speed == Phase.Fast || speed == Phase.Slow;
	#endregion IActionFactory

}