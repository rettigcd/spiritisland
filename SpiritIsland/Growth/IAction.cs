using System.Linq;

namespace SpiritIsland {


	public interface IAction {

		void Apply();

		bool IsResolved {get;}

		IOption[] Options { get; }

		void Select(IOption option);

	}

	public static class IActionExtensions {
		static public void Select(this IAction action, string text)
			=> action.Select(action.Options.Single(o=>o.Text==text));
	}
	
	public interface INamedAction : IAction {
		string Name { get; }
	}

	public interface IOption{
		string Text { get; }
	}

	public class NumberOption : IOption {
		public NumberOption(int i){ 
			Number = i;
			Text = i.ToString();
		}
		public int Number { get; }
		public string Text { get; }
	}

}
