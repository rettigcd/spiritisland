using System.Linq;

namespace SpiritIsland.Core {

	public class SelectGatherDahanSource : IDecision {
		readonly GatherDahanCtx ctx;
		readonly int numberToGather;
		readonly ActionEngine engine;
		readonly bool allowShortCircuit;

		public SelectGatherDahanSource(ActionEngine engine, GatherDahanCtx ctx,int numberToGather,bool allowShortCircuit=false){
			this.engine = engine;
			this.ctx = ctx;
			this.numberToGather = numberToGather;
			this.allowShortCircuit = allowShortCircuit;

			var options = ctx.neighborCounts.Keys.Cast<IOption>().ToList();
			if(allowShortCircuit && options.Count>0)
				options.Add(new TextOption("Done"));				
			this.Options = options.ToArray();
		}
		public IOption[] Options {get;}

		public string Prompt => "Select source land to gather Dahan into "+ctx.Target.Label;

		public void Select( IOption option ) {
			Space source = (Space)option;
			// update ctx
			if(source != ctx.Target){
				++ctx.DestinationCount;
				--ctx.neighborCounts[source];
			}

			// add official move
			engine.actions.Add(new MoveDahan(source,ctx.Target));
			int remaining=numberToGather-1;
			if(remaining>0 && ctx.neighborCounts.Keys.Any())
				engine.decisions.Push(new SelectGatherDahanSource(engine,ctx,remaining,allowShortCircuit));
		}

	}

}



