namespace SpiritIsland;

public interface IExecuteOn<CTX> {

	/// <summary>
	/// Dual Purpose: 
	/// 1) detects if action can be performed on context
	/// 2) detects if pre-requisite condition has been triggered
	/// </summary>
	bool IsApplicable(CTX ctx);

	/// <summary>
	/// Describes the action that will be taken. (ie. Push 2 Beast)
	/// </summary>
	/// <remarks>This is not the Title of a card unless the Title provides a description of the action.</remarks>
	string Description { get; }

	Task Execute(CTX ctx);
}

public class DecisionOption<T> : IExecuteOn<T> {

	#region constructors

	public DecisionOption( string description, Func<T,Task> action ) {
		Description = description;
		asyncFunc = action;
	}

	public DecisionOption( string description, Action<T> syncAction ) {
		Description = description;
		asyncFunc = ctx => { syncAction(ctx); return Task.CompletedTask; };
	}

	#endregion

	/// <summary>
	/// Criteria is pre-evaluated once (probably for Pick1(...) and passed in.
	/// </summary>
	public DecisionOption<T> OnlyExecuteIf( bool condition ) {
		if(!condition)
			isApplicable = (_)=> false;
		return this;
	}

	/// <summary>
	/// Checks if Target matches criteria for executing action.
	/// May be re-evaluated any # of times.
	/// </summary>
	public DecisionOption<T> OnlyExecuteIf( Predicate<T> predicate ) {
		isApplicable = predicate;
		return this;
	}

	public string Description { get; }

	public bool IsApplicable( T ctx ) 
		=> isApplicable == null  // not specified
		|| isApplicable(ctx); // matches

	public Task Execute( T ctx ) => asyncFunc(ctx);

	readonly Func<T,Task> asyncFunc;
	Predicate<T> isApplicable;
}

public class SelfAction : DecisionOption<SelfCtx> {
	public SelfAction( string description, Func<SelfCtx, Task> action ) : base( description, action ) { }
	public SelfAction( string description, Action<SelfCtx> action ) : base( description, action ) { }

	// - new -
	public new SelfAction OnlyExecuteIf(bool condition ) => (SelfAction)base.OnlyExecuteIf( condition );
	public new SelfAction OnlyExecuteIf(Predicate<SelfCtx> predicate ) => (SelfAction)base.OnlyExecuteIf( predicate );
}

public class SpaceAction : DecisionOption<TargetSpaceCtx> {
	public SpaceAction( string description, Func<TargetSpaceCtx, Task> action ) : base( description, action ) { }
	public SpaceAction( string description, Action<TargetSpaceCtx> action ) : base( description, action ) { }

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

public class PickSpaceAction : DecisionOption<TargetSpaceCtx> {
	public PickSpaceAction( params DecisionOption<TargetSpaceCtx>[] actions ) 
		: base( "Select action:" + actions.Select(a=>a.Description).Join(", "), 
				ctx => ctx.SelectActionOption( actions ) 
		) {
	}
}

public class OtherAction : DecisionOption<TargetSpiritCtx> {
	public OtherAction( string description, Func<TargetSpiritCtx, Task> action ) : base( description, action ) { }
	public OtherAction( string description, Action<TargetSpiritCtx> action ) : base( description, action ) { }

	// - new -
//		public new OtherAction CondStatic(bool condition ) => (OtherAction)base.CondStatic( condition );
//		public new OtherAction Cond(Predicate<TargetSpiritCtx> predicate ) => (OtherAction)base.Cond( predicate );
}
