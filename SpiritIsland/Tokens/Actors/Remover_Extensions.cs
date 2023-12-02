namespace SpiritIsland;

static public class Remover_Extensions {

	#region Removing (vanilla)

	/// <summary> Removes tokens from SourceSelector. (optional) </summary>
	static public Task RemoveUpToN(this SourceSelector ss, Spirit decisionMaker ) => ss.RemoveN(decisionMaker,Present.Done);

	/// <summary> Removes tokens from SourceSelector. (defaults to removing all) </summary>
	static public async Task RemoveN(this SourceSelector ss, Spirit decisionMaker, Present present = Present.Always ) {
		await foreach( SpaceToken token in ss.PromptForRemoving( decisionMaker, present ) )
			await token.Remove();
	}

	static public IAsyncEnumerable<SpaceToken> PromptForRemoving(this SourceSelector ss, Spirit self, Present present )
		=> ss.GetEnumerator( self, Prompt.RemainingParts("Remove"), present );

	#endregion Removing (vanilla)

	#region Destroying

	/// <summary> Destroys tokens from SourceSelector. (optional) </summary>
	static public Task DestroyUpToN(this SourceSelector ss, Spirit decisionMaker ) => ss.DestroyN(decisionMaker,Present.Done);

	/// <summary> Destroys tokens from SourceSelector. (defaults to removing all) </summary>
	static public async Task DestroyN(this SourceSelector ss, Spirit decisionMaker, Present present = Present.Always ) {
		await foreach( SpaceToken token in ss.PromptForDestroying( decisionMaker, present ) )
			await token.Remove();
	}

	static public IAsyncEnumerable<SpaceToken> PromptForDestroying(this SourceSelector ss, Spirit self, Present present )
		=> ss.GetEnumerator( self, Prompt.RemainingParts("Destroy"), present );

	#endregion

}

