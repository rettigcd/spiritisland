namespace SpiritIsland;

public interface IIslandTokenApi {

	Task Publish_Adding( AddingTokenArgs args );

	Task Publish_Removing( RemovingTokenArgs args );

	Task Publish_Added( TokenAddedArgs args );

	Task Publish_Removed( TokenRemovedArgs args );

	Task Publish_Moved( TokenMovedArgs args );

	int GetDynamicTokenFor( Space space, UniqueToken token );

	TokenCountDictionary GetTokensFor( Space space );

	HealthToken GetDefault( HealthTokenClass tokenClass );

}