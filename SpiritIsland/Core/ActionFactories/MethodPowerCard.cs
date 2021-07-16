using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	class MethodPowerCard : PowerCard {

		readonly MethodBase m;
		readonly bool targetsSpirit;
		readonly ITargetSpace targetSpace;

		public MethodPowerCard(MethodBase m){
			var attr = m.GetCustomAttributes<PowerCardAttribute>().VerboseSingle();

			// check if targets spirit
			targetsSpirit = m.GetCustomAttributes<TargetSpiritAttribute>().Any();

			targetSpace = (ITargetSpace)m.GetCustomAttributes<FromPresenceAttribute>().FirstOrDefault()
				?? (ITargetSpace)m.GetCustomAttributes<FromSacredSiteAttribute>().FirstOrDefault();

			Speed = attr.Speed;
			Name = attr.Name;
			Cost = attr.Cost;
			Elements = attr.Elements;
			PowerType = attr.PowerType;
			this.m = m;
		}

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			if(targetsSpirit)
				return new MethodAction_TargetSpirit(spirit,gameState,m);
			if(targetSpace!=null)
				return new MethodAction_TargetSpace(spirit,gameState,m,targetSpace);
			return new GenericMethodAction(spirit,gameState,m);
		}

		class GenericMethodAction : BaseAction {
			public GenericMethodAction(
				Spirit self,
				GameState gameState,
				MethodBase m
			):base(self,gameState)
			{
				m.Invoke(null,new object[]{engine});
			}
		}

		class MethodAction_TargetSpirit : BaseAction {
			public MethodAction_TargetSpirit(
				Spirit self,
				GameState gameState,
				MethodBase m
			):base(self,gameState)
			{
				_ = TargetSpirit(m);
			}
			async Task TargetSpirit(MethodBase m){
				var target = await engine.SelectSpirit();
				m.Invoke(null,new object[]{engine,target});
			}
		}

		class MethodAction_TargetSpace : BaseAction {
			public MethodAction_TargetSpace(
				Spirit self,
				GameState gameState,
				MethodBase m,
				ITargetSpace targetSpace
			):base(self,gameState)
			{
				_ = TargetSpirit(m,targetSpace);
			}
			async Task TargetSpirit(MethodBase m,ITargetSpace targetSpace){
				var target = await targetSpace.Target(engine.Self,engine.Api);
				m.Invoke(null,new object[]{engine,target});
			}
		}


	}

}