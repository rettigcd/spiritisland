namespace SpiritIsland;

/// <summary>
/// Adds AutoRun to ActionFactory so Growth Commands can auto-run
/// </summary>
public interface IHelpGrowActionFactory : IActionFactory {
	bool AutoRun { get; }
}

// Marker for Commands that can Auto-run as part of a Growth Option
public interface ICanAutoRun {}

/// <summary>
/// Wraps a SelfCmd turning it into a GrowthAction
/// </summary>
public class GrowthAction( IActOn<Spirit> cmd, Phase phase = Phase.Growth ) : IHelpGrowActionFactory {
	public readonly IActOn<Spirit> Cmd = cmd; // !!! switch to IActOn<Spirit>

	string IOption.Text => Cmd.Description;

	string IActionFactory.Title => Cmd.Description;
	Task IActionFactory.ActivateAsync( Spirit self ) => Cmd.ActAsync( self );
	bool IActionFactory.CouldActivateDuring( Phase speed, Spirit spirit ) => speed == phase;
	bool IHelpGrowActionFactory.AutoRun => Cmd is ICanAutoRun;
}
