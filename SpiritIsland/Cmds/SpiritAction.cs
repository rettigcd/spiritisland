namespace SpiritIsland;

/// <summary>
/// Command that executes on a Spirit (SelfCtx)
/// Adds OnlyExecuteIf to BaseCmd
/// </summary>
public class SpiritAction : BaseCmd<Spirit> {

	static public BaseCmd<Spirit> NoAction => new SpiritAction("No Action",_=>{});

	#region constructors
	public SpiritAction( string description, Func<Spirit, Task> action ) : base( description, action ) { }
	public SpiritAction( string description, Action<Spirit> action ) : base( description, action ) { }
	/// <summary> For derived types that define behavior by overriding ActAsync() </summary>
	protected SpiritAction( string descript ):base( descript ) { }
	/// <summary> For derived types that define behavior by overriding Description and ActAsync() </summary>
	protected SpiritAction() : base() { }
	#endregion constructors

	#region Conditionals (return SpiritAction)
	// - new -
	public new SpiritAction OnlyExecuteIf( bool condition ) => (SpiritAction)base.OnlyExecuteIf( condition );
	public new SpiritAction OnlyExecuteIf( Predicate<Spirit> predicate ) => (SpiritAction)base.OnlyExecuteIf( predicate );
	#endregion Conditionals (return SpiritAction)

}