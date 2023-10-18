namespace SpiritIsland;

public class BaseCmd<T> : IExecuteOn<T> {

	#region constructors

	public BaseCmd( string description, Func<T,Task> action ) {
		Description = description;
		asyncFunc = action;
	}

	public BaseCmd( string description, Action<T> syncAction ) {
		Description = description;
		asyncFunc = ctx => { syncAction(ctx); return Task.CompletedTask; };
	}

	#endregion

	/// <summary>
	/// If false, disabled Command.
	/// </summary>
	/// <remarks>
	/// Criteria is pre-evaluated once (probably for Pick1(...) and passed in.
	/// </remarks>
	public BaseCmd<T> OnlyExecuteIf( bool condition ) {
		if(!condition)
			_isApplicable = (_)=> false;
		return this;
	}

	/// <summary> If predicate evaluates to false, command does not execute. </summary>
	/// <remarks> May be re-evaluated any # of times.</remarks>
	public BaseCmd<T> OnlyExecuteIf( Predicate<T> predicate ) {
		_isApplicable = predicate;
		return this;
	}

	public string Description { get; }

	public bool IsApplicable( T ctx ) 
		=> _isApplicable == null  // not specified
		|| _isApplicable(ctx); // matches

	public Task Execute( T ctx ) => asyncFunc(ctx);

	readonly Func<T,Task> asyncFunc;
	Predicate<T> _isApplicable;
}
