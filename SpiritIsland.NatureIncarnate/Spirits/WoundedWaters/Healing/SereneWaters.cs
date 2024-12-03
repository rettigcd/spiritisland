namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Implements both the Healing Card && the Island-Wide Token that applies the effect.
/// </summary>
class SereneWaters : IHealingCard {

	const string Name = "Serene Waters";

	static SpecialRule Rule => new SpecialRule(Name, "When your Powers: add/move any # of (A) Invaders or (B) Dahan into your lands, you may Downgrade 1 Invaders. (max 1/Power per A & B).");

	public string Text => Name;

	bool IHealingCard.MeetsRequirement( WoundedWatersBleeding spirit ) {
		return 2 <= spirit.HealingMarkers[Element.Water]
			&& 3 <= spirit.HealingMarkers.Total
			&& !spirit.HealingCardClaimed;
	}

	void IHealingCard.Claim( WoundedWatersBleeding spirit ) {
		GameState.Current.AddIslandMod( new Mod(spirit) );
		spirit.AddSpecialRule(Rule);
	}

	public bool IsClaimed( WoundedWatersBleeding spirit ) => spirit.SpecialRules.Any( r => r.Title == Name );

	class Mod( Spirit spirit ) : BaseModEntity, IHandleTokenAdded {
		readonly Spirit _spirit = spirit;

		async Task IHandleTokenAdded.HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
			// When your powers...
			if(!_spirit.ActionIsMyPower) return;

			// ...into your lands...
			if(!_spirit.Presence.IsOn( to )) return;

			var scope = ActionScope.Current;

			// ...move any number of Invaders into your lands
			const string invaderDowngradeKey = Name+":Invader downgrade";
			if(args.Added.HasTag(TokenCategory.Invader) && !scope.ContainsKey( invaderDowngradeKey )) {
				// you may Downgrade 1 of those invaders (max once per Power)
				await ReplaceInvader.DowngradeSelectedInvader( to, (HumanToken)args.Added );
				scope[invaderDowngradeKey] = true;
			}

			// ...add or move any number o Dahan into one of your lands, you may Downgrade 1 Invader there (max oncer per power)
			const string dahanDowngradeKey = Name + ":Dahan cause downgrade";
			if(args.Added.HasTag(TokenCategory.Dahan) && !scope.ContainsKey( dahanDowngradeKey )) {
				// you may Downgrade 1 of those invaders (max once per Power)
				await ReplaceInvader.Downgrade1( _spirit, to, Present.Done, Human.Invader );
				scope[dahanDowngradeKey] = true;
			}

		}
	}

}

