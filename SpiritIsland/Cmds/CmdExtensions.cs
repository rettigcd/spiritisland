namespace SpiritIsland;

public static class CmdExtensions {
	static public BaseCmd<T> Repeat<T>( this BaseCmd<T> action, int repeatCount )
		=> new BaseCmd<T>( action.Description + "x"+repeatCount, async ctx => {
			for(int i = 0; i<repeatCount;++i)
				await new BaseCmd<T>( action.Description+$" ({i+1} of {repeatCount})", action.Execute ).Execute( ctx );
		} );
}
