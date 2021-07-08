using System;
using System.Collections.Generic;
using System.Linq;

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



}
