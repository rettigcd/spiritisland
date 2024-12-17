namespace SpiritIsland;

public sealed class Healer : IRunWhenTimePasses {

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	public Task TimePasses( GameState gameState ) {
		foreach(Space ss in ActionScope.Current.Spaces_Unfiltered)
			HealSpace( ss );
		_skipInvadersOn.Clear();
		_skipDahanOn.Clear();
		return Task.CompletedTask;
	}

	public void HealSpace( Space space ) {
		// Invaders
		if( !_skipInvadersOn.Contains(space) ){
			HealGroup( Human.Town );
			HealGroup( Human.City );
		}

		// Dahan
		if( !_skipDahanOn.Contains(space) )
			HealGroup( Human.Dahan );

		void HealGroup( HumanTokenClass group ) {
			foreach(HumanToken humanToken in space.HumanOfTag( group ).ToArray())
				if(0<humanToken.FullDamage)
					space.AllHumans(humanToken).Adjust(x=>x.Healthy);
		}

	}

	public void SkipInvadersOn( Space space ) => _skipInvadersOn.Add( space );
	public void SkipDahanOn( Space space ) => _skipDahanOn.Add( space );

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	readonly HashSet<Space> _skipInvadersOn = [];
	readonly HashSet<Space> _skipDahanOn = [];

}
