using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IIslandTokenApi {

		Task Publish_Added( Space space, Token token, int countToAdd, AddReason reason );

		Task Publish_Removed( Space space, Token token, int count, RemoveReason reason );

		Task Publish_Moved( Token token, Space source, Space destination );

		int GetDynamicTokenFor( Space space, UniqueToken token );

		TokenCountDictionary GetTokensFor( Space space );

	}


}
