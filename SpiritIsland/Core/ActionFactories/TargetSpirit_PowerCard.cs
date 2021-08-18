using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary>
	/// ActionFactory based on Method-implemented powers
	/// </summary>
	class TargetSpirit_PowerCard : PowerCard {

		readonly MethodBase methodBase;

		public TargetSpirit_PowerCard(MethodBase methodBase){
			var attr = methodBase.GetCustomAttributes<BaseCardAttribute>()
				.VerboseSingle("bob22");

			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;
			PowerType = attr.PowerType;
			this.methodBase = methodBase;
		}

		public override async Task Activate( Spirit self, GameState gameState ) {
			Spirit target = await self.SelectSpirit(gameState.Spirits);
			SpiritTargeted?.Invoke( self, this, target );
			TargetSpirit( methodBase, self, gameState, target );
		}

		public event SpiritTargetedArgs SpiritTargeted; // Targeter, Card, Targetee


		static public void TargetSpirit(MethodBase methodBase, Spirit self, GameState gameState, Spirit target) => methodBase.Invoke( null, new object[] { self.BindSpiritActions(gameState), target } );

	}

	public delegate void SpiritTargetedArgs( Spirit initiator, PowerCard card, Spirit target );

}