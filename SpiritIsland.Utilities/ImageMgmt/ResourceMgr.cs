namespace SpiritIsland;

/// <summary> 
/// Manages the Life-Time of a Resource.  Calls its .Dispose() so caller does not need to know who owns the handle.
/// Caller should ALWAYS dispose of the ResourceMgr and the Resource-Manager will know if it needs to dispose of the Resource or not.
/// </summary>
/// <param name="_owned">True => client will dispose it.  False => supplier manages Dispose. </param>
public sealed class ResourceMgr<TResource>( TResource resource, bool _owned ) : IDisposable 
	where TResource : IDisposable
{
	public TResource Resource => resource;
	public void Dispose() { if(_owned && resource is not null) resource.Dispose(); }
}
