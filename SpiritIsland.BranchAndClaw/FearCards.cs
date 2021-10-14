using System.Linq;

namespace SpiritIsland.BranchAndClaw {

	public class FearCards {

		public static IFearOptions[] GetFearCards() {

			return typeof(FearCards).Assembly.GetTypes()
				.Where( t => t.IsAssignableTo(typeof(DahanAttack) ) )
				.Select( t => (IFearOptions)System.Activator.CreateInstance(t) )
				.ToArray();

			//return new IFearOptions[] {
			//	new DahanAttack(),
			//	new DahanThreaten(),
			//	new Demoralized(),
			//	new DepartTheDangerousLand(),
			//	new Discord(),
			//	new ExplorersAreReluctant(),
			//	new FleeThePestilentLand(),
			//	new ImmigrationSlows(),
			//	new Panic(),
			//	new PanickedByWildBeasts(),
			//	new PlanForDeparture(),
			//	new Quarantine(),
			//	new TooManyMonsters(),
			//	new TreadCarefully(),
			//	new Unrest()
			//};
		}

	}


}
