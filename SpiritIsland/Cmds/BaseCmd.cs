namespace SpiritIsland;

/// <summary>
/// Implements IActOn and adds Conditionals
/// </summary>
public class BaseCmd<T> : IActOn<T> {

	static public BaseCmd<T> Null => new BaseCmd<T>("No Action", _=>Task.CompletedTask );

	#region constructors

	public BaseCmd( string description, Func<T,Task> func ) {
		Description = description;
		_func = func;
	}

	public BaseCmd( string description, Action<T> action ) {
		Description = description;
		_action = action;
	}

	/// <summary>
	/// Used for by derieved types that override ActAsync ActAsync()
	/// </summary>
	protected BaseCmd( string description ) { Description = description; }

	/// <summary>
	/// Used for by derieved types that override Description and ActAsync()
	/// </summary>
	protected BaseCmd() {}

	#endregion

	public virtual string Description { get; }

	/// <remarks>Virtual so that growth actions that inherit from SelfCmd can more easily define the execute</returns>
	public virtual Task ActAsync( T ctx) {
		if ( _func is not null ) return _func(ctx);
		_action(ctx);
		return Task.CompletedTask;
	}

	#region Add/Read conditionals

	/// <summary>
	/// If false, disabled Command.
	/// </summary>
	/// <remarks>
	/// Criteria is pre-evaluated once (probably for Pick1(...) and passed in.
	/// </remarks>
	public BaseCmd<T> OnlyExecuteIf( bool condition ) {
		if(!condition)
			_isApplicable = ( _ ) => false;
		return this;
	}

	/// <summary> If predicate evaluates to false, command does not execute. </summary>
	/// <remarks> May be re-evaluated any # of times.</remarks>
	public BaseCmd<T> OnlyExecuteIf( Predicate<T> predicate ) {
		_isApplicable = predicate;
		return this;
	}

	public bool IsApplicable( T ctx )
		=> _isApplicable == null  // not specified
		|| _isApplicable( ctx ); // matches

	#endregion

	#region private
	readonly Func<T,Task> _func;
	readonly Action<T> _action;
	Predicate<T> _isApplicable;
	#endregion
}
