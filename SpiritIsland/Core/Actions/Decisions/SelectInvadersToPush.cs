using System.Linq;

namespace SpiritIsland.Core {

	public class SelectInvadersToPush : IDecision {

		readonly InvaderGroup invaderGroup;
		readonly int count;
		readonly string[] labels;
		readonly ActionEngine engine;
		readonly bool allowShortCircuit;

		public SelectInvadersToPush(ActionEngine engine, InvaderGroup invaderGroup,int count,bool allowShortCircuit, params string[] labels){
			this.engine = engine;
			this.invaderGroup = invaderGroup;
			this.count = count;
			this.labels = labels;
			this.allowShortCircuit = allowShortCircuit;

		}

		public string Prompt => $"Select invader to push.";

		public IOption[] Options => options ??= CalcOptions();
		IOption[] options;

		IOption[] CalcOptions(){
			// MUST be lazy loaded since this is pushed onto the endinge.decisions stack
			// BEFORE the explorer is actually removed from the target land.

			var options = invaderGroup
				.InvaderTypesPresent
				.Where(i=>labels.Contains(i.Label))
				.Cast<IOption>()
				.ToList();

			if(allowShortCircuit && options.Count>0 )
				options.Add(new TextOption("Done"));

 			return options.ToArray();
		}

		public void Select( IOption option ) {

			if(option is TextOption txt && txt.Text == "Done")
				return;

			// if we need more, push next
			if(count>1)
				engine.decisions.Push(new SelectInvadersToPush( engine, invaderGroup,count-1,allowShortCircuit,labels));

			// select where to push this invader
			engine.decisions.Push( new SelectInvaderDestination( engine, invaderGroup, (Invader)option ) );
		}

	}

}
