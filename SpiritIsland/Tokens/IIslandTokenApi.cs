namespace SpiritIsland;

public interface IIslandTokenApi {

	Task Publish_Adding( AddingTokenArgs args );

	Task Publish_Removing( RemovingTokenArgs args );

	Task Publish_Added( TokenAddedArgs args );

	Task Publish_Removed( PublishTokenRemovedArgs args );

	Task Publish_Moved( TokenMovedArgs args );

	int GetDynamicTokensFor( SpaceState space, UniqueToken token );

	SpaceState GetTokensFor( Space space );

	HealthToken GetDefault( HealthTokenClass tokenClass );

	SpaceState this[Space space] { get; }
	IEnumerable<SpaceState> PowerUp( IEnumerable<Space> spaces );

}