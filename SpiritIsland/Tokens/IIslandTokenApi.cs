namespace SpiritIsland;

public interface IIslandTokenApi {

	Task Publish_Moved( TokenMovedArgs args );

	int GetDynamicTokensFor( SpaceState space, TokenClassToken token );

	IToken GetDefault( IEntityClass tokenClass );

	SpaceState this[Space space] { get; }
	SpaceState GetTokensFor( Space space );

	int InvaderAttack( HumanTokenClass tokenClass );

	void Adjust( ITrackMySpaces token, Space space, int delta );
	bool IsOn( ITrackMySpaces token, Board board );

}