using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class SelectTextAsync : IDecision {

		readonly TaskCompletionSource<string> promise;

		public SelectTextAsync(string prompt, IEnumerable<string> options,TaskCompletionSource<string> promise ){
			Prompt = prompt;
			Options = options.Select(o=>new TextOption(o)).ToArray();
			this.promise = promise;
		}

		public string Prompt {get;}

		public IOption[] Options {get;}

		public void Select( IOption option ) 
			=> promise.TrySetResult(option.Text);

	}



}
