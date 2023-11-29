namespace SpiritIsland;

public static class Strifing_Extensions {
	/// <summary>
	/// Builds an enumerator that Prompts for Strifing
	/// </summary>
	static public IAsyncEnumerable<SpaceToken> PromptForStrifingAll(this SourceSelector ss, Spirit spirit )
		=> ss.GetEnumerator(spirit,Prompt.RemainingParts("Add Strife"), Present.Always );

	static public async Task StrifeAll(this SourceSelector ss, Spirit spirit ) {
		await foreach(SpaceToken before in ss.PromptForStrifingAll(spirit) )
			await before.Add1StrifeToAsync();
	}

}