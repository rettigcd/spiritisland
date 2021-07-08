using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class SelectText : IDecision {

		readonly TextOption[] options;
		readonly Action<string,ActionEngine> selectAction;
		readonly ActionEngine engine;

		public SelectText(ActionEngine engine, Action<string,ActionEngine> selectAction, IEnumerable<string> options ){
			this.engine = engine;
			this.options = options.Select(o=>new TextOption(o)).ToArray();
			this.selectAction = selectAction;
		}

		public SelectText(ActionEngine engine, Action<string,ActionEngine> selectAction, params string[] options){
			this.engine = engine;
			this.options = options.Select(o=>new TextOption(o)).ToArray();
			this.selectAction = selectAction;
		}


		public string Prompt => "Select Innate option";

		public IOption[] Options => options;

		public void Select( IOption option ) 
			=> selectAction(((TextOption)option).Text,engine);

	}


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
