namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds/Removes a single Beast token for a Sacred Site
/// </summary>
public class ManyMindsToken : SpiritPresenceToken, IHandleTokenAdded, IHandleTokenRemoved {

	#region constructor

	public ManyMindsToken() {
		_beastToken = new ManyMindsBeast(this);
	}

	#endregion

	Task IHandleTokenAdded.HandleTokenAdded( ITokenAddedArgs args ){
		UpdateBeastCount( args.Token, args.AddedTo );
		return Task.CompletedTask;
	}

	Task IHandleTokenRemoved.HandleTokenRemoved( ITokenRemovedArgs args ){
		UpdateBeastCount( args.Token, args.RemovedFrom );
		return Task.CompletedTask;
	}

	void UpdateBeastCount( IToken token, SpaceState ss ) {
		if(token != this) return;
		int beastCount = ss[this] < 2 ? 0 : 1;
		ss.Init(_beastToken,beastCount);
	}

	#region private fields
	readonly ManyMindsBeast _beastToken;
	#endregion
}
