namespace SpiritIsland;

public interface IIslandTokenApi {

	int GetDynamicTokensFor( SpaceState space, TokenClassToken token );

	IToken GetDefault( IEntityClass tokenClass );

	void Adjust( ITrackMySpaces token, Space space, int delta );

}