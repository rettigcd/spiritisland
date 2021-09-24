using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary>
	/// ActionFactory based on Method-implemented powers
	/// </summary>
	public class PowerCard_TargetSpace : PowerCard {

		readonly TargetSpaceAttribute targetSpaceAttr;

		public PowerCard_TargetSpace( MethodBase methodBase, TargetSpaceAttribute targetSpace ):base(methodBase){

			targetSpaceAttr = targetSpace ?? throw new ArgumentNullException( nameof( targetSpace ), Name + " is missing targetting attribute" );

			Name = cardAttr.Name;
			Cost = cardAttr.Cost;
			Speed = cardAttr.Speed;
			Elements = cardAttr.Elements;
			PowerType = cardAttr.PowerType;

		}

		public override Task ActivateAsync( Spirit spirit, GameState gameState ) {
			return PickSpaceAndActivate(spirit,gameState);
		}

		public Task ActivateAgainstSpecificTarget( Spirit spirit, GameState gameState, Space preTarget ) {
			return Task.Run( ()=>InvokeAgainst( spirit, gameState, preTarget) );
		}

		async Task PickSpaceAndActivate( Spirit spirit, GameState gameState ) {
			var target = await targetSpaceAttr.GetTarget( spirit, gameState );
			if(target == null) return; // no space available that meets criteria.
			await InvokeAgainst( spirit, gameState, target );
		}

		Task InvokeAgainst( Spirit spirit, GameState gameState, Space target) 
			=> (Task)methodBase.Invoke( null, new object[] { new TargetSpaceCtx( spirit, gameState, target, Cause.Power) } );

	}

}