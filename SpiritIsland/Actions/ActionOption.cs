using System;
using System.Threading.Tasks;


namespace SpiritIsland {

	public interface IExecuteOn<CTX> {
		bool IsApplicable { get; }
		string Description { get; }
		Task Execute(CTX ctx);
	}

	public class ActionOption<T> : IExecuteOn<T> {

		#region constructors

		public ActionOption( string description, Func<T,Task> action, bool applicable = true ) {
			Description = description;
			IsApplicable = applicable;
			asyncFunc = action;
		}

		public ActionOption( string description, Action<T> syncAction, bool applicable = true ) {
			Description = description;
			IsApplicable = applicable;
			asyncFunc = ctx => { syncAction(ctx); return Task.CompletedTask; };
		}

		#endregion

		public string Description { get; }
		public bool IsApplicable { get; }
		public Task Execute( T ctx ) => asyncFunc(ctx);
		readonly Func<T,Task> asyncFunc;
	}

	public class SelfAction
		: ActionOption<SelfCtx> 
		, IExecuteOn<TargetSpiritCtx>
		, IExecuteOn<TargetSpaceCtx>
	{
		public SelfAction( string description, Func<SelfCtx, Task> action, bool applicable = true ) : base( description, action, applicable ) { }
		public SelfAction( string description, Action<SelfCtx> action, bool applicable = true ) : base( description, action, applicable ) { }

		Task IExecuteOn<TargetSpiritCtx>.Execute( TargetSpiritCtx ctx ) => this.Execute( ctx );
		Task IExecuteOn<TargetSpaceCtx>.Execute( TargetSpaceCtx ctx ) => this.Execute( ctx );
	}

	public class SpaceAction : ActionOption<TargetSpaceCtx> {
		public SpaceAction( string description, Func<TargetSpaceCtx, Task> action, bool applicable = true ) : base( description, action, applicable ) { }
		public SpaceAction( string description, Action<TargetSpaceCtx> action, bool applicable = true ) : base( description, action, applicable ) { }
	}

	public class OtherAction : ActionOption<TargetSpiritCtx> {
		public OtherAction( string description, Func<TargetSpiritCtx, Task> action, bool applicable = true ) : base( description, action, applicable ) { }
		public OtherAction( string description, Action<TargetSpiritCtx> action, bool applicable = true ) : base( description, action, applicable ) { }
	}

}