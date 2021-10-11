using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary>
	/// ActionFactory based on Method-implemented powers
	/// </summary>
	class PowerCard_TargetSpirit : PowerCard {

		public PowerCard_TargetSpirit(MethodBase methodBase):base(methodBase){}

		public override async Task ActivateAsync( Spirit self, GameState gameState ) {
			Spirit target = gameState.Spirits.Length == 1
				? gameState.Spirits[0]
				: await self.Action.Decision(new Decision.TargetSpirit(gameState.Spirits));

			SpiritTargeted?.Invoke( self, this, target );
			var ctx = new TargetSpiritCtx(self,gameState,target,Cause.Power);
			await TargetSpirit( methodBase, ctx );
		}

		public event SpiritTargetedArgs SpiritTargeted; // Targeter, Card, Targetee

		static public Task TargetSpirit(MethodBase methodBase, TargetSpiritCtx ctx ) {
			return (Task)methodBase.Invoke( null, new object[] { ctx } );
		}

	}

	public delegate void SpiritTargetedArgs( Spirit initiator, PowerCard card, Spirit target );

}