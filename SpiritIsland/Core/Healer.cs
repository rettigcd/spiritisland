namespace SpiritIsland;

public sealed class Healer : IRunWhenTimePasses {

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	public Task TimePasses( GameState gameState ) {
		foreach(SpaceState ss in ActionScope.Current.Tokens_Unfiltered)
			HealSpace( ss );
		_skipInvadersOn.Clear();
		_skipDahanOn.Clear();
		return Task.CompletedTask;
	}

	public void HealSpace( SpaceState tokens ) {
		// Invaders
		if( !_skipInvadersOn.Contains(tokens) ){
			HealGroup( Human.Town );
			HealGroup( Human.City );
		}

		// Dahan
		if( !_skipDahanOn.Contains(tokens) )
			HealGroup( Human.Dahan );

		void HealGroup( HumanTokenClass group ) {
			foreach(HumanToken humanToken in tokens.HumanOfTag( group ).ToArray())
				if(0<humanToken.FullDamage)
					tokens.AllHumans(humanToken).Adjust(x=>x.Healthy);				
		}

	}

	public void SkipInvadersOn( SpaceState space ) => _skipInvadersOn.Add( space );
	public void SkipDahanOn( SpaceState space ) => _skipDahanOn.Add( space );

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	readonly HashSet<SpaceState> _skipInvadersOn = [];
	readonly HashSet<SpaceState> _skipDahanOn = [];

}
