using SpiritIsland.PowerCards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class SelectText : IDecision {

		readonly TextDecision[] options;
		readonly Action<string,ActionEngine> selectAction;

		public SelectText(IEnumerable<string> options, Action<string,ActionEngine> selectAction){
			this.options = options.Select(o=>new TextDecision(o)).ToArray();
			this.selectAction = selectAction;
		}

		public string Prompt => "Select Innate option";

		public IOption[] Options => options;

		public void Select( IOption option, ActionEngine engine ) 
			=> selectAction(((TextDecision)option).Text,engine);

		class TextDecision : IOption {
			public TextDecision(string text){ Text = text; }
			public string Text { get; }
		}

	}


}
