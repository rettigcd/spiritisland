using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SelectAsync<T> : IDecisionMaker where T:class,IOption {

		readonly TaskCompletionSource<T> promise;
		readonly Present present;

		public IDecisionPlus Decision { get; }

		public SelectAsync(
			string prompt,
			T[] options,
			Present present,
			TaskCompletionSource<T> promise
		){
			this.promise = promise;

			var optionList = options.Cast<IOption>().ToList();
			if(optionList.Count == 0)
				promise.TrySetResult(null);
			else if(present == Present.Done)
				optionList.Add(TextOption.Done);

			Decision = new DecisionInner {
				Prompt = prompt,
				Options = optionList.ToArray(),
				AllowAutoSelect = present != Present.Always,
			};

		}

		class DecisionInner : IDecisionPlus {
			public bool AllowAutoSelect {get; set; }
			public string Prompt {get;set; }
			public IOption[] Options {get;set;}
		}

		public void Select( IOption option ) {
			if(/*present == Present.Done &&*/ TextOption.Done.Matches(option))
				promise.TrySetResult(null);
			else if(Decision.Options.Contains(option))
				promise.TrySetResult((T)option);
			else
				promise.TrySetException(new Exception($"{option.Text} not found in options"));
		}

	}

	public enum Present {
		/// <summary> Shows for 1 or more items.</summary>
		Always,

		/// <summary>
		/// Shows for 2 or more options
		/// </summary>
		IfMoreThan1,

		/// <summary>
		/// Shows for 1 or more items, plus has done, cancel
		/// </summary>
		Done,
	}

}