using System;

namespace SpiritIsland.PowerCards {
	class Decision : IDecision {
		public Decision( Func<IOption[]> options, Action<IOption,ActionEngine> select ){
			this.options = options;
			this.select = select;
		}

		readonly Func<IOption[]> options;
		readonly Action<IOption,ActionEngine> select;

		public IOption[] Options => options();
		public void Select(IOption option, ActionEngine engine){
			select(option,engine);
			Selection = option;
			this.Selected?.Invoke( option );
		}
		public IOption Selection {get; private set;}

		public string Prompt => "";

		event Action<IOption> Selected;
	}

}
