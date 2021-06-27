
namespace SpiritIsland.Core {
	class ResolvedAction : IAction {

		System.Action apply;

		public ResolvedAction(System.Action apply){
			this.apply = apply;
		}

		void IAction.Apply() => apply();

		void IAction.Select( IOption option ) {
			throw new System.NotImplementedException();
		}

		bool IAction.IsResolved => true;

		IOption[] IAction.Options => new IOption[0];
	}

}
