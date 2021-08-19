using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SelectAsync<T> : IDecisionPlus where T:class,IOption {

		readonly TaskCompletionSource<T> promise;
		readonly Present present;

		public SelectAsync(
			string prompt,
			T[] options,
			Present present,
			TaskCompletionSource<T> promise
		){
			Prompt = prompt;
			this.promise = promise;
			this.present = present;

			var optionList = options.Cast<IOption>().ToList();
			if(optionList.Count == 0)
				promise.TrySetResult(null);
			else if(present == Present.Done)
				optionList.Add(TextOption.Done);
			Options = optionList.ToArray();
		}

		public string Prompt {get;}

		public IOption[] Options {get;}

		public bool AllowAutoSelect => present != Present.Always;

		public void Select( IOption option ) {
			if(present == Present.Done && TextOption.Done.Matches(option))
				promise.TrySetResult(null);
			else
				promise.TrySetResult((T)option);
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