namespace SpiritIsland;

public class Healer : IRunWhenTimePasses {

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	public virtual Task TimePasses( GameState gameState ) {
		foreach(SpaceState ss in gameState.Spaces_Unfiltered)
			HealSpace( ss );
		_skipInvadersOn.Clear();
		return Task.CompletedTask;
	}

	public virtual void HealSpace( SpaceState tokens ) {
		// Invaders
		if( !_skipInvadersOn.Contains(tokens.Space) ){
			HealGroup( Human.Town );
			HealGroup( Human.City );
		}

		// Dahan
		HealGroup( Human.Dahan );

		void HealGroup( HumanTokenClass group ) {
			foreach(HumanToken humanToken in tokens.HumanOfTag( group ).ToArray())
				if(0<humanToken.FullDamage)
					tokens.AllHumans(humanToken).Adjust(x=>x.Healthy);				
		}

	}

	public void SkipInvadersOn( Space space ) => _skipInvadersOn.Add( space );

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	protected HashSet<Space> _skipInvadersOn = new HashSet<Space>();

}
