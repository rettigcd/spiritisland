namespace SpiritIsland;

public interface IIslandTokenApi {

	int GetDynamicTokensFor( SpaceState space, TokenClassToken token );

	IToken GetDefault( IEntityClass tokenClass );

	int InvaderAttack( HumanTokenClass tokenClass );

	void Adjust( ITrackMySpaces token, Space space, int delta );
	bool IsOn( ITrackMySpaces token, Board board );

}