namespace SpiritIsland;

public class Healer : IRunWhenTimePasses {

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	public virtual Task TimePasses( GameState gameState ) {
		foreach(SpaceState ss in gameState.Spaces_Unfiltered)
			HealSpace( ss );
		skipHealSpaces.Clear();
		return Task.CompletedTask;
	}

	public virtual void HealSpace( SpaceState tokens ) {
		if( !skipHealSpaces.Contains(tokens.Space) )
			InvaderBinding.HealTokens( tokens );
	}

	public void Skip( Space space ) => skipHealSpaces.Add( space );

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	protected HashSet<Space> skipHealSpaces = new HashSet<Space>();

}
