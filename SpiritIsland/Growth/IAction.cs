namespace SpiritIsland {


	public interface IAction {

		void Apply();

		bool IsResolved {get;}

		IOption[] GetOptions();

		void Select(IOption option);

	}
	
	public interface INamedAction : IAction {
		string Name { get; }
	}

	public interface IOption{
		string Text { get; }
	}

}
