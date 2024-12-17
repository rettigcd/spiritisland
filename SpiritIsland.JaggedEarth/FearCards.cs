namespace SpiritIsland.JaggedEarth;

public class FearCards {

	public static IFearCard[] GetFearCards() {

		return typeof(FearCards).Assembly.GetTypes()
			.Where( t => t.IsAssignableTo(typeof(BesetByManyTroubles) ) )
			.Select( t => (IFearCard)(Activator.CreateInstance(t)!) )
			.ToArray();

	}

}