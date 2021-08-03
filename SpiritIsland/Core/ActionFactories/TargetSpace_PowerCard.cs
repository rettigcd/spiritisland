using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	/// <summary>
	/// ActionFactory based on Method-implemented powers
	/// </summary>
	class TargetSpace_PowerCard : PowerCard {

		readonly MethodBase methodBase;
		readonly TargetSpaceAttribute targetSpace;

		public TargetSpace_PowerCard(MethodBase methodBase,TargetSpaceAttribute targetSpace){
			this.methodBase = methodBase;
			this.targetSpace = targetSpace;

			var attr = methodBase.GetCustomAttributes<BaseCardAttribute>().VerboseSingle("Couldn't find BaseCardAttribute on PowerCard targeting a space");
			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;
			PowerType = attr.PowerType;
		}

		public event SpaceTargetedEvent TargetedSpace; // Targeter, Card, Targetee

		public override Task Activate( ActionEngine engine ) {
			return PickSpaceAndActivate(engine);
		}

		public Task ActivateAgainstSpecificTarget( ActionEngine engine, Space preTarget ) {
			return Task.Run( ()=>InvokeAgainst(engine,preTarget) );// ?????
		}

		async Task PickSpaceAndActivate( ActionEngine engine ){
			var target = await targetSpace.Target( engine );
			TargetedSpace?.Invoke(new SpaceTargetedArgs{Initiator=engine.Self,Card=this,Target=target } );
			InvokeAgainst( engine, target );
		}

		void InvokeAgainst(ActionEngine engine, Space target) => methodBase.Invoke( null, new object[] { engine, target } );

	}


	public class SpaceTargetedArgs{ public Spirit Initiator; public PowerCard Card; public Space Target; };
	public delegate void SpaceTargetedEvent(SpaceTargetedArgs args);


}