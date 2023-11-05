namespace SpiritIsland;

/// <summary>
/// Command that executes on a Spirit (SelfCtx)
/// </summary>
public class SpiritAction : BaseCmd<SelfCtx> {
	public SpiritAction( string description, Func<SelfCtx, Task> action ) : base( description, action ) { }
	public SpiritAction( string description, Action<SelfCtx> action ) : base( description, action ) { }
	protected SpiritAction( string descript ):base( descript ) { } 

	// - new -
	public new SpiritAction OnlyExecuteIf(bool condition ) => (SpiritAction)base.OnlyExecuteIf( condition );
	public new SpiritAction OnlyExecuteIf(Predicate<SelfCtx> predicate ) => (SpiritAction)base.OnlyExecuteIf( predicate );
}
