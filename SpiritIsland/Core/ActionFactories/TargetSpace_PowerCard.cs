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

		public override void Activate( ActionEngine engine ) {
			TargetSpace_Action.DoIt(engine,m,targetSpace);
		}

		public void Activate( ActionEngine engine, Space preTarget ) {
			TargetSpace_Action.DoIt( engine, m, preTarget );
		}


	}

	class TargetSpace_Action {

		static public void DoIt( ActionEngine engine, MethodBase m, Space preSpace ) {
			var action = new TargetSpace_Action( engine, m ){
				Target = preSpace ?? throw new ArgumentNullException( nameof( preSpace ) )
			};
			Task.Run( action.Invoke );
		}

		TargetSpace_Action( ActionEngine engine, MethodBase m) {
			this.engine = engine;
			this.methodBase = m;
		}


		static public void DoIt( ActionEngine engine, MethodBase m, TargetSpaceAttribute targetSpace ){
			var action = new TargetSpace_Action( engine, m );
			_ = action.TargetSpaceThenInvoke( targetSpace );
		}

		async Task TargetSpaceThenInvoke(TargetSpaceAttribute targetSpace){
			Target = await targetSpace.Target( engine ) 
				?? throw new Exception("await targetspace returned value");
			Invoke();
		}

		void Invoke() => methodBase.Invoke(null,new object[]{engine,Target});

		public Space Target { get; private set; }
		readonly MethodBase methodBase;
		readonly ActionEngine engine;

	}



}