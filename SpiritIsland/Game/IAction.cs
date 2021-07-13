using System.Linq;

namespace SpiritIsland {

	public interface IAction {

		string Prompt { get; }

		IOption[] Options { get; }

		void Select(IOption option);

		bool IsResolved {get;} // Should this be removed and just test if Options length == 0?

		public string Selections {get;}

	}

	public static class IActionExtensions {
		static public void Select(this IAction action, string text)
			=> action.Select(action.Options.Single(o=>o.Text==text));
	}

}
