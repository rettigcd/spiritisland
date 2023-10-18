namespace SpiritIsland;

/// <summary>
/// Command that executes on a Spirit (SelfCtx)
/// </summary>
public class SelfCmd : BaseCmd<SelfCtx> {
	public SelfCmd( string description, Func<SelfCtx, Task> action ) : base( description, action ) { }
	public SelfCmd( string description, Action<SelfCtx> action ) : base( description, action ) { }

	// - new -
	public new SelfCmd OnlyExecuteIf(bool condition ) => (SelfCmd)base.OnlyExecuteIf( condition );
	public new SelfCmd OnlyExecuteIf(Predicate<SelfCtx> predicate ) => (SelfCmd)base.OnlyExecuteIf( predicate );
}
