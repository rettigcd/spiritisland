namespace SpiritIsland;

public interface ILocation : IOption {
	Task<(ITokenAddedArgs,Func<ITokenAddedArgs,Task>)> SinkAsync( IToken token, int count=1, AddReason addReason = AddReason.Added );
	Task<(ITokenRemovedArgs,Func<ITokenRemovedArgs,Task>)> SourceAsync( IToken token, int count, RemoveReason reason = RemoveReason.Removed );
}