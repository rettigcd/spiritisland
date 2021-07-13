
namespace SpiritIsland.Core {
	class ResolvedAction : IAction {

		void IAction.Select( IOption option ) {
			throw new System.NotImplementedException();
		}

		bool IAction.IsResolved => true;

		IOption[] IAction.Options => System.Array.Empty<IOption>();

		public string Prompt => "-";

		public string Selections => "-pre-resolved-";
	}

}
