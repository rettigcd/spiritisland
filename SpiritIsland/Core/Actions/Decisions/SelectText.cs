using SpiritIsland.PowerCards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	public class SelectText : IDecision {

		readonly TextOption[] options;
		readonly Action<string,ActionEngine> selectAction;

		public SelectText(IEnumerable<string> options, Action<string,ActionEngine> selectAction){
			this.options = options.Select(o=>new TextOption(o)).ToArray();
			this.selectAction = selectAction;
		}

		public string Prompt => "Select Innate option";

		public IOption[] Options => options;

		public void Select( IOption option, ActionEngine engine ) 
			=> selectAction(((TextOption)option).Text,engine);

	}


}
