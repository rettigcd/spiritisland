namespace SpiritIsland;

public interface IIslandTokenApi {

	Task Publish_Moved( TokenMovedArgs args );

	int GetDynamicTokensFor( SpaceState space, UniqueToken token );

	HealthToken GetDefault( HealthTokenClass tokenClass );

	IEnumerable<SpaceState> PowerUp( IEnumerable<Space> spaces );

	SpaceState this[Space space] { get; }
	SpaceState GetTokensFor( Space space );

	GameState AccessGameState();

	int InvaderAttack( HealthTokenClass tokenClass );

}