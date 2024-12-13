namespace SpiritIsland;

public class TokenRemovedHandlerAsync(Func<ITokenRemovedArgs, Task> handler)
	: BaseModEntity()
	, IEndWhenTimePasses
	, IHandleTokenRemoved
{
	Task IHandleTokenRemoved.HandleTokenRemovedAsync(ITokenRemovedArgs _args) => handler(_args);
}

public class TokenRemovedHandlerAsync_Persistent(Func<ITokenRemovedArgs, Task> _handler)
	: BaseModEntity
	, IHandleTokenRemoved {
	Task IHandleTokenRemoved.HandleTokenRemovedAsync(ITokenRemovedArgs args) => _handler(args);

}

public class TokenRemovedHandler(Action<ITokenRemovedArgs> _handler)
	: BaseModEntity()
	, IEndWhenTimePasses
	, IHandleTokenRemoved
{
	Task IHandleTokenRemoved.HandleTokenRemovedAsync(ITokenRemovedArgs args) {
		_handler(args);
		return Task.CompletedTask;
	}
}