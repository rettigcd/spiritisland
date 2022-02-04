namespace SpiritIsland;

public interface IExecuteOn<CTX> {
	bool IsApplicable(CTX ctx);
	string Description { get; }
	Task Execute(CTX ctx);
}

public class ActionOption<T> : IExecuteOn<T> {

	#region constructors

	public ActionOption( string description, Func<T,Task> action ) {
		Description = description;
		asyncFunc = action;
	}

	public ActionOption( string description, Action<T> syncAction ) {
		Description = description;
		asyncFunc = ctx => { syncAction(ctx); return Task.CompletedTask; };
	}

	#endregion

	/// <summary>
	/// Criteria is pre-evaluated once (probably for Pick1(...) and passed in.
	/// </summary>
	public IExecuteOn<T> FilterOption( bool condition ) {
		if(!condition)
			isApplicable = (_)=> false;
		return this;
	}

	/// <summary>
	/// Checks if Target matches criteria for executing action.
	/// May be re-evaluated any # of times.
	/// </summary>
	public IExecuteOn<T> Matches( Predicate<T> predicate ) {
		isApplicable = predicate;
		return this;
	}

	public string Description { get; }
	public bool IsApplicable( T ctx ) => isApplicable == null  // not specified
		|| isApplicable(ctx);

	public Task Execute( T ctx ) => asyncFunc(ctx);

	readonly Func<T,Task> asyncFunc;
	Predicate<T> isApplicable;
}

public class SelfAction
	: ActionOption<SelfCtx> 
	, IExecuteOn<TargetSpiritCtx>
	, IExecuteOn<TargetSpaceCtx>
	, IExecuteOn<BoardCtx>
{
	public SelfAction( string description, Func<SelfCtx, Task> action ) : base( description, action ) { }
	public SelfAction( string description, Action<SelfCtx> action ) : base( description, action ) { }

	Task IExecuteOn<TargetSpiritCtx>.Execute( TargetSpiritCtx ctx ) => this.Execute( ctx );
	Task IExecuteOn<TargetSpaceCtx>.Execute( TargetSpaceCtx ctx ) => this.Execute( ctx );
	Task IExecuteOn<BoardCtx>.Execute( BoardCtx ctx ) => this.Execute( ctx );

	bool IExecuteOn<TargetSpiritCtx>.IsApplicable( TargetSpiritCtx ctx ) => this.IsApplicable( ctx );
	bool IExecuteOn<TargetSpaceCtx>.IsApplicable( TargetSpaceCtx ctx ) => this.IsApplicable( ctx );
	bool IExecuteOn<BoardCtx>.IsApplicable( BoardCtx ctx ) => this.IsApplicable( ctx );

	// - new -
	public new SelfAction FilterOption(bool condition ) => (SelfAction)base.FilterOption( condition );
	public new SelfAction Matches(Predicate<SelfCtx> predicate ) => (SelfAction)base.Matches( predicate );
}

public class SpaceAction : ActionOption<TargetSpaceCtx> {
	public SpaceAction( string description, Func<TargetSpaceCtx, Task> action ) : base( description, action ) { }
	public SpaceAction( string description, Action<TargetSpaceCtx> action ) : base( description, action ) { }

	// - new -

	/// <summary>
	/// Use this for single-evaluate in Pick1(..) selection criteria.
	/// Not meant to be evaluated multiple times.
	/// </summary>
	public new SpaceAction FilterOption(bool condition ) => (SpaceAction)base.FilterOption( condition );

	/// <summary>
	/// Used this for checking sutability a Space.
	/// May be evaluated multiple times.
	/// </summary>
	public new SpaceAction Matches(Predicate<TargetSpaceCtx> predicate ) => (SpaceAction)base.Matches( predicate );
}

public class PickSpaceAction : ActionOption<TargetSpaceCtx> {
	public PickSpaceAction( params ActionOption<TargetSpaceCtx>[] actions ) 
		: base( "Select action:" + actions.Select(a=>a.Description).Join(", "), 
				ctx => ctx.SelectActionOption( actions ) 
		) {
	}
}

public class OtherAction : ActionOption<TargetSpiritCtx> {
	public OtherAction( string description, Func<TargetSpiritCtx, Task> action ) : base( description, action ) { }
	public OtherAction( string description, Action<TargetSpiritCtx> action ) : base( description, action ) { }

	// - new -
//		public new OtherAction CondStatic(bool condition ) => (OtherAction)base.CondStatic( condition );
//		public new OtherAction Cond(Predicate<TargetSpiritCtx> predicate ) => (OtherAction)base.Cond( predicate );
}
