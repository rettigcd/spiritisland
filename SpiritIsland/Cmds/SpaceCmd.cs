namespace SpiritIsland;

/// <summary>
/// Cmd that executes on a given Space
/// </summary>
/// <remarks>
/// Wraps BaseCmd-TargetSpaceCtx- for ease of use.
/// </remarks>
public class SpaceCmd : BaseCmd<TargetSpaceCtx> {
	public SpaceCmd( string description, Func<TargetSpaceCtx, Task> action ) : base( description, action ) { }
	public SpaceCmd( string description, Action<TargetSpaceCtx> action ) : base( description, action ) { }

	// - new -

	/// <summary>
	/// If, condition fails to match, command is not executed.
	/// Use this for single-evaluate in Pick1(..) selection criteria.
	/// </summary>
	/// <returns>self for chaining</returns>
	public new SpaceCmd OnlyExecuteIf(bool condition) => (SpaceCmd)base.OnlyExecuteIf( condition );

	/// <summary>
	/// If, condition fails to match, command is not executed.
	/// </summary>
	/// <remarks>
	/// Intended to be called from SelectActionOption to know if action is valid for this space or not.
	/// </remarks>
	/// <returns>self for chaining</returns>
	public new SpaceCmd OnlyExecuteIf( Predicate<TargetSpaceCtx> predicate ) => (SpaceCmd)base.OnlyExecuteIf( predicate );
}
