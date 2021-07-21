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

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new TargetSpirit_Action(spirit,gameState,m);
		}

	}

	class TargetSpirit_Action : BaseAction {

		public Spirit Target {get; private set; }

		public TargetSpirit_Action( 
			Spirit self, 
			GameState gameState, 
			MethodBase m
		) :base(self,gameState) {
			this.methodBase = m;
			_ = FindSpiritAndInvoke();
		}
		async Task FindSpiritAndInvoke(){
			Target = await engine.SelectSpirit();
			Invoke();
		}

		public TargetSpirit_Action( Spirit self, GameState gameState, MethodBase m, Spirit target ):base(self,gameState) {
			this.methodBase = m;
			Target = target;
			Task.Run(Invoke);
		}

		void Invoke() {
			methodBase.Invoke(null, new object[] { engine, Target } );
		}

		readonly MethodBase methodBase;

	}


}