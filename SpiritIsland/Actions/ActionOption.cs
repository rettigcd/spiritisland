using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ActionOption {

		#region constructors

		/// <summary> Used for TargetSpiritCtx </summary>
		public ActionOption( string description, Action action ) {
			Description = description;
			this.action = action;
			IsApplicable = true;
		}

		public ActionOption( string description, Func<Task> action ) {
			Description = description;
			func = action;
			IsApplicable = true;
		}

		public ActionOption( string description, Func<TargetSpaceCtx,Task> action ) {
			Description = description;
			targetSpaceFunc = action;
			IsApplicable = true;
		}

		public ActionOption( string description, Action<TargetSpaceCtx> action ) {
			Description = description;
			targetSpaceAction = action;
			IsApplicable = true;
		}

		public ActionOption( string description, Action action, bool applicable ) {
			Description = description;
			this.action = action;
			IsApplicable = applicable;
		}

		public ActionOption( string description, Func<Task> action, bool applicable ) {
			Description = description;
			func = action;
			IsApplicable = applicable;
		}

		public ActionOption( string description, Func<TargetSpaceCtx,Task> action, bool applicable ) {
			Description = description;
			targetSpaceFunc = action;
			IsApplicable = applicable;
		}

		public ActionOption( string description, Action<TargetSpaceCtx> action, bool applicable ) {
			Description = description;
			targetSpaceAction = action;
			IsApplicable = applicable;
		}



		#endregion

		public string Description { get; }
		public bool IsApplicable { get; }

		public Task Execute(){
			if(targetSpaceFunc != null) throw new InvalidOperationException();
			if( func!=null )
				return func();
			action();
			return Task.CompletedTask;
		}

		public Task Execute(TargetSpaceCtx ctx){
			if( targetSpaceFunc!=null )
				return targetSpaceFunc(ctx);
			if(targetSpaceAction != null) {
				targetSpaceAction(ctx);
				return Task.CompletedTask;
			}

			return Execute();
		}

		readonly Func<TargetSpaceCtx,Task> targetSpaceFunc;
		readonly Func<Task> func;

		readonly Action<TargetSpaceCtx> targetSpaceAction;
		readonly Action action;

	}


}