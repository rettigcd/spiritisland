//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SpiritIsland {

//	/// <summary>
//	/// Replaces the DrawCard Growth Action when receiving a Major Power Progression Card.
//	/// </summary>
//	public class ForgetPowerCard : IActionFactory {

//		public Speed Speed => Speed.Growth;

//		public string Name => "Forget Power Card";

//		public string Text => Name;

//		public IActionFactory Original => this;

//		public Task Activate( ActionEngine engine ) {
//			return engine.ForgetPowerCard();
//		}

//	}

//}
