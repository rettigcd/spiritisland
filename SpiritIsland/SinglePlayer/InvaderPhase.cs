//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SpiritIsland.SinglePlayer {

//	public class InvaderPhase {

//		public InvaderPhase(GameState gameState){
//			this.gameState = gameState;
//			gameState.NewInvaderLogEntry += GameState_NewInvaderLogEntry;
//		}

//		void GameState_NewInvaderLogEntry( string msg ) {
//			NewLogEntry?.Invoke( msg );
//		}

//		public event Action<string> NewLogEntry;

//		public async Task ActAsync() {
//			await gameState.InvaderEngine.DoInvaderPhase();
//		}

//		#region private fields

//		readonly GameState gameState;

//		#endregion




//	}

//}
