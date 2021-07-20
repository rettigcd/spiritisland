using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	/// <summary>
	/// ActionFactory based on Method-implemented powers
	/// </summary>
	class TargetSpace_PowerCard : PowerCard {

		readonly MethodBase m;
		readonly TargetSpaceAttribute targetSpace;

		public TargetSpace_PowerCard(MethodBase m,TargetSpaceAttribute targetSpace){
			this.m = m;
			this.targetSpace = targetSpace;

			var attr = m.GetCustomAttributes<BaseCardAttribute>().VerboseSingle("bob 555");
			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;
			PowerType = attr.PowerType;
		}

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new TargetSpace_Action(spirit,gameState,m,targetSpace);
		}

	}

	class TargetSpace_Action : BaseAction {

		public TargetSpace_Action( Spirit self, GameState gameState, MethodBase m, TargetSpaceAttribute targetSpace):base(self,gameState) {
			this.methodBase = m;
			_ = TargetSpaceThenInvoke(targetSpace);
		}

		public TargetSpace_Action( Spirit self, GameState gameState, MethodBase m, Space target):base(self,gameState) {
			this.methodBase = m;
			Target = target ?? throw new ArgumentNullException(nameof(target));
			Task.Run(Invoke);
		}

		async Task TargetSpaceThenInvoke(TargetSpaceAttribute targetSpace){
			Target = await targetSpace.Target( engine ) 
				?? throw new Exception("await targetspace returned value");
			Invoke();
		}

		void Invoke() => methodBase.Invoke(null,new object[]{engine,Target});

		public Space Target { get; private set; }

		readonly MethodBase methodBase;
	}


}