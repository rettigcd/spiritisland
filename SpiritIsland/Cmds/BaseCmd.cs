namespace SpiritIsland;

public class BaseCmd<T> : IActOn<T> {

	#region constructors

	public BaseCmd( string description, Func<T,Task> action ) {
		Description = description;
		asyncFunc = action;
	}

	public BaseCmd( string description, Action<T> syncAction ) {
		Description = description;
		asyncFunc = ctx => { syncAction(ctx); return Task.CompletedTask; };
	}

	/// <summary>
	/// Used for by derieved types that override the Execute command
	/// </summary>
	protected BaseCmd( string description ) {
		Description = description;
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

	/// <remarks>Virtual so that growth actions that inherit from SelfCmd can more easily define the execute</returns>
	public virtual Task ActAsync( T ctx ) => asyncFunc(ctx);

	readonly Func<T,Task> asyncFunc;
	Predicate<T> _isApplicable;
}
