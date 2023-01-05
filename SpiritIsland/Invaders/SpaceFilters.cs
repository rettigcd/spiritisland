
namespace SpiritIsland;

public interface SpaceFilter {
	bool Matches( Space space );
	string Text { get; }
}
public class SpaceFilters {
	public static bool WithDahanAndExplorers( SpaceState space ) => WithDahan( space ) && WithExplorers( space );
	public static bool WithDahanAndInvaders( SpaceState space ) => WithDahan( space ) && WithInvaders( space );

	public static bool WithExplorers( SpaceState space ) => space.Has( Invader.Explorer );
	public static bool WithInvaders( SpaceState space ) => space.HasInvaders();
	public static bool WithDahan( SpaceState space ) => space.Dahan.Any;
	public static bool WithStrife( SpaceState s ) => s.Keys.OfType<HealthToken>().Any( x => x.StrifeCount > 0 );
	public static bool HasBeastOrIsAdjacentToBeast( SpaceState space ) => space.Range( 1 )
		.Any( x => HasBeast( x ) );
	public static bool HasBeast( SpaceState space ) => space.Beasts.Any;


}