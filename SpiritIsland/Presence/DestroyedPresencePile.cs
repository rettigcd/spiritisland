namespace SpiritIsland;

public class DestroyedPresencePile : ILocation {

	public static readonly DestroyedPresencePile Singleton = new DestroyedPresencePile();
	
	public Task<(ITokenAddedArgs, Func<ITokenAddedArgs, Task>)> 
	SinkAsync( IToken token, int count, AddReason addReason = AddReason.Added ) {
		// !!! what about Incarna, can it be destroyed this way?
		if(token is not SpiritPresenceToken spt) throw new ArgumentException("Can only Sink Presence Tokens");
		var destroyed = spt.Self.Presence.Destroyed;
		destroyed.Count += count;
		return Task.FromResult<(ITokenAddedArgs,Func<ITokenAddedArgs,Task>)>((
			new TokenAddedArgs(spt,this,count,AddReason.AddedToCard),
			_ => Task.CompletedTask
        ));
	}
	
	public Task<(ITokenRemovedArgs, Func<ITokenRemovedArgs, Task>)> 
	SourceAsync( IToken token, int count, RemoveReason reason = RemoveReason.Removed ) {
		if(token is not SpiritPresenceToken spt) throw new ArgumentException("Can only Source Presence Tokens");
		var destroyed = spt.Self.Presence.Destroyed;
		count = Math.Min(count,destroyed.Count);
		spt.Self.Presence.Destroyed.Count -= count;
		return Task.FromResult<(ITokenRemovedArgs,Func<ITokenRemovedArgs,Task>)>((
			new TokenRemovedArgs(this,spt,count,RemoveReason.TakingFromCard),
			_ => Task.CompletedTask
        ));
	}

}

