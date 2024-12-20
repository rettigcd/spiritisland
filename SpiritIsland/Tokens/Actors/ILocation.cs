namespace SpiritIsland;

public interface ILocation : IOption {

	// To Move a token, use IToken.MoveAsync(source,destination)

	/// <returns>The Add Result plus the notification callback.</returns>
	/// <remarks>The notification callback is included so Move events can be published as a single unit in bothe lands.</remarks>
	Task<(ITokenAddedArgs,Func<ITokenAddedArgs,Task>)> SinkAsync( IToken token, int count=1, AddReason addReason = AddReason.Added );

	/// <returns>The Remove Result plus the notification callback.</returns>
	/// <remarks>The notification callback is included so Move events can be published as a single unit in bothe lands.</remarks>
	Task<(ITokenRemovedArgs,Func<ITokenRemovedArgs,Task>)> SourceAsync( IToken token, int count, RemoveReason reason = RemoveReason.Removed );
}