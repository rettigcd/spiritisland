using System;
using System.Linq;
using System.Reflection;

namespace SpiritIsland.Core {

	public class InnatePower : IActionFactory {

		#region Constructors and factories

		static public InnatePower For<T>(){ 
			Type actionType = typeof(T);
			InnatePowerAttribute innatePowerAttr = actionType.GetCustomAttribute<InnatePowerAttribute>();

			TargetSpaceAttribute targetSpace = (TargetSpaceAttribute)actionType.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
				?? (TargetSpaceAttribute)actionType.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();

			// try static method (spirit / major / minor)
			var method = actionType.GetMethods(BindingFlags.Public|BindingFlags.Static)
				.Where(m=>m.GetCustomAttributes<InnateOptionAttribute>().Count()==1)
				.VerboseSingle("Expect 1 innate methods per class"); // 1 method per class - for now

			bool targetSpirit = method.GetCustomAttributes<TargetSpiritAttribute>().Any();

			return new InnatePower(innatePowerAttr,method,targetSpirit,targetSpace);
		}

		readonly MethodBase method;
		readonly bool targetSpirit;
		readonly TargetSpaceAttribute targetSpace;

		internal InnatePower(InnatePowerAttribute innatePowerAttr, MethodBase method,bool targetSpirit, TargetSpaceAttribute targetSpace){
			this.method = method;
			this.targetSpirit = targetSpirit;
			this.targetSpace = targetSpace;

			Speed = innatePowerAttr.Speed;
			Name = innatePowerAttr.Name;

			powerLevels = method.GetCustomAttributes<InnateOptionAttribute>().ToArray();
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

		readonly InnateOptionAttribute[] powerLevels;

		public IAction Bind( Spirit spirit, GameState gameState ) {
			if(targetSpirit)
				return new TargetSpirit_Action(spirit,gameState,method);

			return new TargetSpace_Action(spirit,gameState,method,targetSpace);
		}

	}

}