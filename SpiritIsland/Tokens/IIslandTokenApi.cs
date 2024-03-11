namespace SpiritIsland;

public interface IIslandTokenApi {

	int GetDynamicTokensFor( Space space, TokenClassToken token );

	HumanToken GetDefault( ITokenClass tokenClass );

}