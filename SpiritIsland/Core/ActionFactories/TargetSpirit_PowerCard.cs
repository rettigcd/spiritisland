using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	/// <summary>
	/// ActionFactory based on Method-implemented powers
	/// </summary>
	class TargetSpirit_PowerCard : PowerCard {

		readonly MethodBase m;

		public TargetSpirit_PowerCard(MethodBase m){
			var attr = m.GetCustomAttributes<BaseCardAttribute>()
				.VerboseSingle("bob22");

			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;
			PowerType = attr.PowerType;
			this.m = m;
		}

		public override void Activate( ActionEngine engine ) {
			TargetSpirit_Action.FindSpiritAndInvoke(engine,m);
		}

	}

	class TargetSpirit_Action {

		public static void FindSpiritAndInvoke(ActionEngine engine, MethodBase m ) {
			var x = new TargetSpirit_Action( engine, m );
			_ = x.FindSpiritAndInvoke();
		}

		// not used
		//public static void InvokeOnSpirit(ActionEngine engine, MethodBase m, Spirit target){
		//	var x = new TargetSpirit_Action( engine, m ) {
		//		Target = target
		//	};
		//	Task.Run( x.Invoke );
		//}

		Spirit target;

		TargetSpirit_Action(  ActionEngine engine, MethodBase m ) {
			this.engine = engine;
			this.methodBase = m;
		}

		async Task FindSpiritAndInvoke(){
			target = await engine.SelectSpirit();
			Invoke();
		}

		void Invoke() {
			methodBase.Invoke(null, new object[] { engine, target } );
		}

		readonly MethodBase methodBase;
		readonly ActionEngine engine;

	}

}