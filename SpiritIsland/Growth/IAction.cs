namespace SpiritIsland {


	public interface IAction {

		void Apply();

		bool IsResolved {get;}

	}
	
	public interface INamedAction : IAction {
		string Name { get; }
	}

}
