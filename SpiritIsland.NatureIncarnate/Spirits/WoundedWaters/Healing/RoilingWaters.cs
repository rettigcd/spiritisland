namespace SpiritIsland.NatureIncarnate;

class RoilingWaters : IHealingCard {
	const string Name = "Roiling Waters";
	public string Text => Name;

	static readonly SpecialRule Rule = new SpecialRule(Name, "When your Powers add or move Beast into a land, you may do 1 Damage there per added or moved Beast. When your Powers add or move any number of Dahan into a land, you may do 1 Damage there (max once per Power)" );

	public bool MeetsRequirement( WoundedWatersBleeding spirit )
		=> 2 <= spirit.HealingMarkers[Element.Animal]
		&& 3 <= spirit.HealingMarkers.Total
		&& !spirit.HealingCardClaimed;

	public void Claim( WoundedWatersBleeding spirit ) {
		GameState.Current.AddIslandMod( new Mod(spirit) );
		spirit.AddSpecialRule( Rule );
	}

	public bool IsClaimed( WoundedWatersBleeding spirit ) => spirit.SpecialRules.Any(r=>r.Title==Name);

	class Mod : BaseModEntity, IHandleTokenAddedAsync {
		readonly Spirit _spirit;
		public Mod(Spirit spirit ) { _spirit = spirit; }
		async Task IHandleTokenAddedAsync.HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args ) {
			// When your powers...
			if(!_spirit.ActionIsMyPower) return;

			var scope = ActionScope.Current;

			// ...add or move Beast into a land
			const string beastDamageKey = Name+":BeastDamage";

			if(args.Added == Token.Beast && !scope.ContainsKey( beastDamageKey )) {
				// you may do 1 Damage there per added or moved Beast.
				await to.UserSelected_DamageInvaders( _spirit, args.Count, Human.Invader );
				scope[ beastDamageKey ] = true;
			}

			// add or move any number of Dahan into a land
			const string dahanDamageKey = Name + ":DahanDamage";
			if(args.Added.HasTag(TokenCategory.Dahan) && !scope.ContainsKey( dahanDamageKey )) {
				// you may do 1 Damage there (max once per Power)
				await to.UserSelected_DamageInvaders( _spirit, 1, Human.Invader );
				scope[ dahanDamageKey ] = true;
			}
		}

	}

}
