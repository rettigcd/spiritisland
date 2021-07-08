//using System.Linq;

//namespace SpiritIsland.Core {
//	public class SelectInvaderDestination : IDecision {

//		readonly InvaderGroup invaderGroup;
//		readonly Invader invader;
//		readonly ActionEngine engine;

//		public SelectInvaderDestination(ActionEngine engine,InvaderGroup invaderGroup, Invader invader){
//			this.engine = engine;
//			this.invaderGroup = invaderGroup;
//			this.invader = invader;
//		}

//		public IOption[] Options => invaderGroup.Space.Neighbors
//			.Where(x=>x.IsLand)
//			.ToArray();

//		public void Select( IOption option ) {
//			engine.actions.Add(new MoveInvader(invader, invaderGroup.Space, (Space)option));
//			invaderGroup[invader]--;
//		}

//		public string Prompt => $"Select land to push {invader.Summary} to.";


//	}

//}
