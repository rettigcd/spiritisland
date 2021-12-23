using System;
using System.Linq;
using System.Threading.Tasks;


namespace SpiritIsland {

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

		public IExecuteOn<T> Cond( bool condition ) {
			if(!condition)
				isApplicable = (_)=> false;
			return this;
		}

		public IExecuteOn<T> Cond( Predicate<T> predicate ) {
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
		public new SelfAction Cond(bool condition ) => (SelfAction)base.Cond( condition );
		public new SelfAction Cond(Predicate<SelfCtx> predicate ) => (SelfAction)base.Cond( predicate );
	}

	public class SpaceAction : ActionOption<TargetSpaceCtx> {
		public SpaceAction( string description, Func<TargetSpaceCtx, Task> action ) : base( description, action ) { }
		public SpaceAction( string description, Action<TargetSpaceCtx> action ) : base( description, action ) { }

		// - new -
		public new SpaceAction Cond(bool condition ) => (SpaceAction)base.Cond( condition );
		public new SpaceAction Cond(Predicate<TargetSpaceCtx> predicate ) => (SpaceAction)base.Cond( predicate );
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
		public new OtherAction Cond(bool condition ) => (OtherAction)base.Cond( condition );
		public new OtherAction Cond(Predicate<TargetSpiritCtx> predicate ) => (OtherAction)base.Cond( predicate );
	}

}