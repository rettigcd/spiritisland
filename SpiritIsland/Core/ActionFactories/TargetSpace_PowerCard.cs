using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary>
	/// ActionFactory based on Method-implemented powers
	/// </summary>
	class TargetSpace_PowerCard : PowerCard {

		readonly MethodBase methodBase;
		readonly TargetSpaceAttribute targetSpaceAttr;

		public TargetSpace_PowerCard(MethodBase methodBase,TargetSpaceAttribute targetSpace){
			this.methodBase = methodBase;
			this.MethodType = methodBase.DeclaringType;

			var attr = methodBase.GetCustomAttributes<CardAttribute>().VerboseSingle("Couldn't find BaseCardAttribute on PowerCard targeting a space");
			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;
			PowerType = attr.PowerType;

			this.targetSpaceAttr = targetSpace ?? throw new ArgumentNullException( nameof( targetSpace ), Name + " is missing targetting attribute" );

		}

		public override Task ActivateAsync( Spirit spirit, GameState gameState ) {
			return PickSpaceAndActivate(spirit,gameState);
		}

		public Task ActivateAgainstSpecificTarget( Spirit spirit, GameState gameState, Space preTarget ) {
			return Task.Run( ()=>InvokeAgainst( spirit, gameState, preTarget) );
		}

		async Task PickSpaceAndActivate( Spirit spirit, GameState gameState ) {
			var target = await targetSpaceAttr.GetTarget( spirit, gameState );
			if(target == null) return; // no space available that meets criteria.   !!! needs unit test showing if no-target-space simply does nothing, and doesn't crash
			await InvokeAgainst( spirit, gameState, target );
		}

		Task InvokeAgainst( Spirit spirit, GameState gameState, Space target) 
			=> (Task)methodBase.Invoke( null, new object[] { new TargetSpaceCtx( spirit, gameState, target) } );

	}

}