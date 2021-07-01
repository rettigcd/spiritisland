using System.Linq;

namespace SpiritIsland {

	public interface IAction {

		// why doesn't this have a string Prompt {get;} like IDecision
		string Prompt { get; }

		void Apply(); // ??? should this be auto-called when Select resolves last option

		bool IsResolved {get;} // Should this be removed and just test if Options length == 0?

		IOption[] Options { get; }

		void Select(IOption option);

	}

	public static class IActionExtensions {
		static public void Select(this IAction action, string text)
			=> action.Select(action.Options.Single(o=>o.Text==text));
	}

}
