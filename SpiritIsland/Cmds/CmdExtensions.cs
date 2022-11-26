namespace SpiritIsland;

public static class CmdExtensions {
	static public DecisionOption<T> Repeat<T>( this DecisionOption<T> action, int repeatCount )
		=> new DecisionOption<T>( action.Description + "x"+repeatCount, async ctx => {
			for(int i = 0; i<repeatCount;++i)
				await new DecisionOption<T>( action.Description+$" ({i+1} of {repeatCount})", action.Execute ).Execute( ctx );
		} );
}
