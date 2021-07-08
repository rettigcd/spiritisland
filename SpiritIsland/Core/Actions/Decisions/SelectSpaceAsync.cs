using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class SelectSpaceAsync : IDecision {

		readonly TaskCompletionSource<Space> promise;

		public SelectSpaceAsync(string prompt, IEnumerable<Space> spaces, bool allowShortCircuit,TaskCompletionSource<Space> promise){
			this.promise = promise;
			Prompt = prompt;
			var options = spaces.Cast<IOption>().ToList();
			if(options.Count == 0) promise.TrySetResult(null);
			if(allowShortCircuit)
				options.Add(TextOption.Done);
			Options = options.ToArray();
		}

		public string Prompt {get;}

		public IOption[] Options {get;}

		public void Select( IOption option ) {
			if(TextOption.Done.Matches(option))
				promise.TrySetResult(null);
			else
				promise.TrySetResult((Space)option);
		}
	}

}