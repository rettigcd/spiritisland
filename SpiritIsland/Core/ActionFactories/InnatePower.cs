using System;
using System.Linq;
using System.Reflection;

namespace SpiritIsland.Core {

	public class InnatePower : IActionFactory {

		#region Constructors and factories

		static public InnatePower For<T>(){ 
			Type actionType = typeof(T);
			// try static method (spirit / major / minor)
			var method = actionType.GetMethods(BindingFlags.Public|BindingFlags.Static)
				.Where(m=>m.GetCustomAttributes<InnatePowerAttribute>().Count()==1)
				.VerboseSingle("Expect 1 innate methods per class"); // 1 method per class - for now

			bool targetSpirit = method.GetCustomAttributes<TargetSpiritAttribute>().Any();

			TargetSpaceAttribute targetSpace = (TargetSpaceAttribute)method.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
				?? (TargetSpaceAttribute)method.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();

			return new InnatePower(method,targetSpirit,targetSpace);
		}

		readonly MethodBase method;
		readonly bool targetSpirit;
		readonly TargetSpaceAttribute targetSpace;

		internal InnatePower(MethodBase method,bool targetSpirit, TargetSpaceAttribute targetSpace){
			this.method = method;
			this.targetSpirit = targetSpirit;
			this.targetSpace = targetSpace;


			var innatePowerAttr = method.GetCustomAttributes<InnatePowerAttribute>().VerboseSingle("bob 123");
			Speed = innatePowerAttr.Speed;
			Name = innatePowerAttr.Name;

			powerLevels = method.GetCustomAttributes<PowerLevelAttribute>().ToArray();
		}

		#endregion

		public int PowersActivated(Spirit spirit){
			bool[] isSubSet = powerLevels
				.Select(pl=>spirit.HasElements(pl.Elements))
				.ToArray();

			return isSubSet.Count(x=>x);
		}

		public Speed Speed {get;}

		public string Name {get;}

		public string Text => Name;

		public IActionFactory Original => this;

		readonly PowerLevelAttribute[] powerLevels;

		public IAction Bind( Spirit spirit, GameState gameState ) {
			if(targetSpirit)
				return new TargetSpirit_Action(spirit,gameState,method);

			return new TargetSpace_Action(spirit,gameState,method,targetSpace);
		}

	}

}