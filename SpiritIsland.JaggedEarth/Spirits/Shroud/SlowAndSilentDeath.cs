namespace SpiritIsland.JaggedEarth;

class SlowAndSilentDeath : IRunWhenTimePasses {

	readonly ShroudOfSilentMist _spirit;

	public SlowAndSilentDeath(ShroudOfSilentMist spirit ) { _spirit = spirit; }

	public static readonly SpecialRule Rule = new SpecialRule(
		"Slow and Silent Death",
		"Invaders and Dahan in your lands don't heal Damage.  During Time Passes: 1 fear (max 5) per land of yours with Damaged Invaders.  Gain 1 Energy per 3 lands of yours with Damaged Invaders."
	);

	bool IRunWhenTimePasses.RemoveAfterRun => false;

	/// <summary> Apply the Skip before the Heal takes place </summary>
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Early;

	Task IRunWhenTimePasses.TimePasses( GameState gameState ){
		var myLands = _spirit.Presence.Lands.ToArray();

		// Invaders and dahan in your lands don't heal Damage.  
		var healer = gameState.Healer;
		foreach(Space myLand in myLands){
			healer.SkipDahanOn(myLand);
			healer.SkipInvadersOn(myLand);
		}

		// During Time Passes:
		static bool IsDamaged(HumanToken i) => i.RemainingHealth < i.FullHealth;			
		var myLandsWithWoundedInvaders = myLands.Tokens()
			.Where( t=>t.InvaderTokens().Any( IsDamaged ) )
			.ToArray();
		// 1 fear (max 5) per land of yours with Damaged Invaders.  
		foreach(var land in myLandsWithWoundedInvaders.Take(5))
			land.AddFear(1);

		// Gain 1 Energy per 3 lands of yours with Damaged Invaders.
		_spirit.Energy += myLandsWithWoundedInvaders.Length / 3;

		return Task.CompletedTask;
	}
}

