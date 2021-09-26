using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary>
	/// ActionFactory based on Method-implemented powers
	/// </summary>
	class PowerCard_TargetSpirit : PowerCard {

		public PowerCard_TargetSpirit(MethodBase methodBase):base(methodBase){
			Speed = cardAttr.Speed;
			Name = cardAttr.Name;
			Cost = cardAttr.Cost;
			Elements = cardAttr.Elements;
			PowerType = cardAttr.PowerType;
		}

		public override async Task ActivateAsync( Spirit self, GameState gameState ) {
			Spirit target = gameState.Spirits.Length == 1
				? gameState.Spirits[0]
				: await self.Action.Decision(new Decision.TargetSpirit(gameState.Spirits));

			SpiritTargeted?.Invoke( self, this, target );
			await TargetSpirit( methodBase, self, gameState, target );
		}

		public event SpiritTargetedArgs SpiritTargeted; // Targeter, Card, Targetee

		static public Task TargetSpirit(MethodBase methodBase, Spirit self, GameState gameState, Spirit target) 
			=> (Task)methodBase.Invoke( null, new object[] { new TargetSpiritCtx(self,gameState,target,Cause.Power) } );

	}

	public delegate void SpiritTargetedArgs( Spirit initiator, PowerCard card, Spirit target );

}