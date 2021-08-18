using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary>
	/// ActionFactory based on Method-implemented powers
	/// </summary>
	class TargetSpace_PowerCard : PowerCard {

		readonly MethodBase methodBase;
		readonly TargetSpaceAttribute targetSpace;

		public TargetSpace_PowerCard(MethodBase methodBase,TargetSpaceAttribute targetSpace){
			this.methodBase = methodBase;

			var attr = methodBase.GetCustomAttributes<BaseCardAttribute>().VerboseSingle("Couldn't find BaseCardAttribute on PowerCard targeting a space");
			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;
			PowerType = attr.PowerType;

			this.targetSpace = targetSpace ?? throw new ArgumentNullException( nameof( targetSpace ), Name + " is missing targetting attribute" );

		}

		public event SpaceTargetedEvent TargetedSpace; // Targeter, Card, Targetee

		public override Task Activate( Spirit spirit, GameState gameState ) {
			return PickSpaceAndActivate(spirit,gameState);
		}

		public Task ActivateAgainstSpecificTarget( Spirit spirit, GameState gameState, Space preTarget ) {
			return Task.Run( ()=>InvokeAgainst( spirit, gameState, preTarget) );
		}

		async Task PickSpaceAndActivate( Spirit spirit, GameState gameState ) {
			var target = await targetSpace.GetTarget( spirit.BindSpiritActions(gameState) );
			if(target == null) return; // no space available that meets criteria.   !!! needs unit test showing if no-target-space simply does nothing, and doesn't crash
			TargetedSpace?.Invoke(new SpaceTargetedArgs{Initiator=spirit,Card=this,Target=target } );
			InvokeAgainst( spirit, gameState, target );
		}

		void InvokeAgainst( Spirit spirit, GameState gameState, Space target) => methodBase.Invoke( null, new object[] { spirit.BindSpiritActions(gameState), target } );

	}

	/// <summary>
	/// When a spirit targets a land, allows us to record this targetting
	/// </summary>
	public class SpaceTargetedArgs { public Spirit Initiator; public PowerCard Card; public Space Target; };

	public delegate void SpaceTargetedEvent( SpaceTargetedArgs args );

}