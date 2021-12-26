using System.Linq;

namespace SpiritIsland.BranchAndClaw {

	public class FearCards {

		public static IFearOptions[] GetFearCards() {

			return typeof(FearCards).Assembly.GetTypes()
				.Where( t => t.IsAssignableTo(typeof(DahanAttack) ) )
				.Select( t => (IFearOptions)System.Activator.CreateInstance(t) )
				.ToArray();

		}

	}

}
