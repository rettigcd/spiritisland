using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

	public class SelectAsync<T> : IDecision where T:class,IOption {

		readonly TaskCompletionSource<T> promise;
		readonly bool allowShortCircuit;

		public SelectAsync(
			string prompt,
			T[] options,
			bool allowShortCircuit,
			TaskCompletionSource<T> promise
		){
			Prompt = prompt;
			this.promise = promise;
			this.allowShortCircuit = allowShortCircuit;

			var optionList = options.Cast<IOption>().ToList();
			if(optionList.Count == 0)
				promise.TrySetResult(null);
			else if(allowShortCircuit)
				optionList.Add(TextOption.Done);
			Options = optionList.ToArray();
		}

		public string Prompt {get;}

		public IOption[] Options {get;}

		public void Select( IOption option ) {
			// !!! need to test that we can short circuit this
			if(allowShortCircuit && TextOption.Done.Matches(option))
				promise.TrySetResult(null);
			else
				promise.TrySetResult((T)option);
		}

	}

}