namespace SpiritIsland.JaggedEarth;

public class FearCards {

	public static IFearOptions[] GetFearCards() {

		return typeof(FearCards).Assembly.GetTypes()
			.Where( t => t.IsAssignableTo(typeof(BesetByManyTroubles) ) )
			.Select( t => (IFearOptions)System.Activator.CreateInstance(t) )
			.ToArray();

	}

}