namespace SpiritIsland;

static public class Remover_Extensions {

	static public IAsyncEnumerable<SpaceToken> PromptForRemoving(this SourceSelector ss, Spirit self, Present present )
		=> ss.GetEnumerator( self, Prompt.RemainingParts("Remove"), present );

	/// <summary>
	/// Defaults to removing all.  use Present.Done to do "UpTo"
	/// </summary>
	static public async Task RemoveN(this SourceSelector ss, Spirit self, Present present = Present.Always ) {
		await foreach( SpaceToken token in ss.PromptForRemoving( self, present ) )
			await token.Remove();
	}

	static public Task RemoveUpToN(this SourceSelector ss, Spirit self ) => ss.RemoveN(self,Present.Done);

}