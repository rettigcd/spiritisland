namespace SpiritIsland;

public class TokenRemovedHandlerAsync( Func<SpaceState, ITokenRemovedArgs, Task> handler ) 
	: BaseModEntity()
	, IEndWhenTimePasses
	, IHandleTokenRemovedAsync 
{
	Task IHandleTokenRemovedAsync.HandleTokenRemovedAsync( SpaceState _from, ITokenRemovedArgs _args ) => handler( _from, _args );
}

public class TokenRemovedHandlerAsync_Persistent( Func<ITokenRemovedArgs, Task> _handler ) 
	: BaseModEntity
	, IHandleTokenRemovedAsync
{
	Task IHandleTokenRemovedAsync.HandleTokenRemovedAsync( SpaceState from, ITokenRemovedArgs args ) => _handler( args );

}

public class TokenRemovedHandler( Action<ITokenRemovedArgs> _handler ) 
	: BaseModEntity()
	, IEndWhenTimePasses
	, IHandleTokenRemoved
{
	void IHandleTokenRemoved.HandleTokenRemoved( SpaceState from, ITokenRemovedArgs args ) {
		_handler( args );
	}
}