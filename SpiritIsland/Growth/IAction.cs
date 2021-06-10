namespace SpiritIsland
{
	public interface IAction {
		void Apply();

		bool IsResolved {get;}

	}


}

// Option 1: Cards only work once they've been 'Bound'
// Option 2: Cards don't bind to user, wrapper does binding.
// Option 3: create 2 classes for each card