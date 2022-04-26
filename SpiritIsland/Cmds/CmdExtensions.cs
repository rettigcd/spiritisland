namespace SpiritIsland;

public static class CmdExtensions {
	static public ActionOption<T> Repeat<T>( this ActionOption<T> action, int repeatCount )
		=> new ActionOption<T>( action.Description + "x"+repeatCount, async ctx => {
			for(int i = 0; i<repeatCount;++i)
				await new ActionOption<T>( action.Description+$" ({i+1} of {repeatCount})", action.Execute ).Execute( ctx );
		} );
}
