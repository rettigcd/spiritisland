namespace SpiritIsland;

public interface IIslandTokenApi {

	Task Publish_Moved( TokenMovedArgs args );

	int GetDynamicTokensFor( SpaceState space, UniqueToken token );

	IVisibleToken GetDefault( TokenClass tokenClass );

	IEnumerable<SpaceState> PowerUp( IEnumerable<Space> spaces );

	SpaceState this[Space space] { get; }
	SpaceState GetTokensFor( Space space );

	int InvaderAttack( HumanTokenClass tokenClass );

	void Adjust( ITrackMySpaces token, Space space, int delta );
	bool IsOn( ITrackMySpaces token, Board board );

}