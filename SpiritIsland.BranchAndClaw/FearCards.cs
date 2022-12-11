namespace SpiritIsland.BranchAndClaw;

public class FearCards {

	public static IFearCard[] GetFearCards() {

		return typeof(FearCards).Assembly.GetTypes()
			.Where( t => t.IsAssignableTo(typeof(DahanAttack) ) )
			.Select( t => (IFearCard)System.Activator.CreateInstance(t) )
			.ToArray();

	}

}