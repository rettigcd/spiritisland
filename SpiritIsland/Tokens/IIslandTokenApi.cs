namespace SpiritIsland;

public interface IIslandTokenApi {

	Task Publish_Added( Space space, Token token, int countToAdd, AddReason reason , Guid actionId);

	Task Publish_Removed( Space space, Token token, int count, RemoveReason reason, Guid actionId );

	Task Publish_Moved( Token token, Space source, Space destination, Guid actionId );

	int GetDynamicTokenFor( Space space, UniqueToken token );

	TokenCountDictionary GetTokensFor( Space space );

	HealthToken GetDefault( HealthTokenClass tokenClass );

}