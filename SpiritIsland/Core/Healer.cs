namespace SpiritIsland;

public sealed class Healer : IRunWhenTimePasses {

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	public Task TimePasses( GameState gameState ) {
		foreach(SpaceState ss in gameState.Spaces_Unfiltered)
			HealSpace( ss );
		_skipInvadersOn.Clear();
		_skipDahanOn.Clear();
		return Task.CompletedTask;
	}

	public void HealSpace( SpaceState tokens ) {
		// Invaders
		if( !_skipInvadersOn.Contains(tokens.Space) ){
			HealGroup( Human.Town );
			HealGroup( Human.City );
		}

		// Dahan
		if( !_skipDahanOn.Contains(tokens.Space) )
			HealGroup( Human.Dahan );

		void HealGroup( HumanTokenClass group ) {
			foreach(HumanToken humanToken in tokens.HumanOfTag( group ).ToArray())
				if(0<humanToken.FullDamage)
					tokens.AllHumans(humanToken).Adjust(x=>x.Healthy);				
		}

	}

	public void SkipInvadersOn( Space space ) => _skipInvadersOn.Add( space );
	public void SkipDahanOn( Space space ) => _skipDahanOn.Add( space );

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	readonly HashSet<Space> _skipInvadersOn = new HashSet<Space>();
	readonly HashSet<Space> _skipDahanOn = new HashSet<Space>();

}
