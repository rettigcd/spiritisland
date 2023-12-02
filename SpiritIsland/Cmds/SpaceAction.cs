namespace SpiritIsland;

/// <summary>
/// Cmd that executes on a given Space
/// </summary>
/// <remarks>
/// Wraps BaseCmd-TargetSpaceCtx- for ease of use.
/// </remarks>
public class SpaceAction : BaseCmd<TargetSpaceCtx> {
	public SpaceAction( string description, Func<TargetSpaceCtx, Task> action ) : base( description, action ) { }
	public SpaceAction( string description, Action<TargetSpaceCtx> action ) : base( description, action ) { }
	
	// Derived types that just override DisplayText and Act...
	protected SpaceAction() : base() { }

	// - new -

	/// <summary>
	/// If, condition fails to match, command is not executed.
	/// Use this for single-evaluate in Pick1(..) selection criteria.
	/// </summary>
	/// <returns>self for chaining</returns>
	public new SpaceAction OnlyExecuteIf(bool condition) => (SpaceAction)base.OnlyExecuteIf( condition );

	/// <summary>
	/// If, condition fails to match, command is not executed.
	/// </summary>
	/// <remarks>
	/// Intended to be called from SelectActionOption to know if action is valid for this space or not.
	/// </remarks>
	/// <returns>self for chaining</returns>
	public new SpaceAction OnlyExecuteIf( Predicate<TargetSpaceCtx> predicate ) => (SpaceAction)base.OnlyExecuteIf( predicate );
}
