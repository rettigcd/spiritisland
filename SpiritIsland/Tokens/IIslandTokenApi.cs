namespace SpiritIsland;

public interface IIslandTokenApi {

	int GetDynamicTokensFor( SpaceState space, TokenClassToken token );

	HumanToken GetDefault( ITokenClass tokenClass );

}