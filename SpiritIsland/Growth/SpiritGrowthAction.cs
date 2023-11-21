namespace SpiritIsland;

/// <summary>
/// Adds AutoRun to ActionFactory so Growth Commands can auto-run
/// </summary>
public interface IHelpGrow : IActionFactory {
	bool AutoRun { get; }
}

// Marker for Commands that can Auto-run as part of a Growth Option
public interface ICanAutoRun {}

/// <summary>
/// Wraps a SelfCmd turning it into a GrowthAction
/// </summary>
public class SpiritGrowthAction : IHelpGrow {
	public readonly IActOn<SelfCtx> Cmd;
	readonly Phase _phase;
	public SpiritGrowthAction( IActOn<SelfCtx> cmd, Phase phase = Phase.Growth ) { Cmd = cmd; _phase = phase; }
	string IActionFactory.Name => Cmd.Description;
	string IOption.Text => Cmd.Description;
	Task IActionFactory.ActivateAsync( SelfCtx ctx ) => Cmd.ActAsync( ctx );
	bool IActionFactory.CouldActivateDuring( Phase speed, Spirit spirit ) => speed == _phase;
	bool IHelpGrow.AutoRun => Cmd is ICanAutoRun;
}

static public class SelfCmdExtender {
	static public SpiritGrowthAction ToInit( this SpiritAction cmd ) => new SpiritGrowthAction( cmd, Phase.Init );
}