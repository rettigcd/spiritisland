namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds/Removes a single Beast token for a Sacred Site
/// </summary>
public class ManyMindsPresenceToken 
	: SpiritPresenceToken
	, IHandleTokenAdded
	, IHandleTokenRemoved {

	#region constructor

	public ManyMindsPresenceToken(Spirit spirit):base(spirit) {
		_beastToken = new ManyMindsBeast(this);
	}

	#endregion

	void IHandleTokenAdded.HandleTokenAdded( SpaceState to, ITokenAddedArgs args ){
		UpdateBeastCount( args.Added, to );
	}

	void IHandleTokenRemoved.HandleTokenRemoved( SpaceState from, ITokenRemovedArgs args ){
		UpdateBeastCount( args.Removed, from );
	}

	void UpdateBeastCount( ISpaceEntity token, SpaceState ss ) {
		if(token != this) return;
		int beastCount = ss[this] < 2 ? 0 : 1;
		ss.Init(_beastToken,beastCount);
	}

	#region private fields
	readonly ManyMindsBeast _beastToken;
	#endregion
}
