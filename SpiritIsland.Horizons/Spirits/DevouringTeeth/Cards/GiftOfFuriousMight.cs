namespace SpiritIsland.Horizons;

public class GiftOfFuriousMight {

	public const string Name = "Gift of Furious Might";

	[SpiritCard(Name, 1, Element.Moon, Element.Fire,Element.Animal), Fast, AnotherSpirit]
	[Instructions("Once this turn, Target Spirit may deal +3 Damage when using a Damage-dealing Power."), Artist(Artists.MoroRogers)]
	static public Task ActAsync(TargetSpiritCtx ctx) {
		// Once this turn, Target Spirit may deal +3 Damage when using a Damage-dealing Power.
		GameState.Current.AddIslandMod(new OneTimeDamageBoost(ctx.Other,3));
		return Task.CompletedTask;
	}

}

class OneTimeDamageBoost(Spirit spirit, int damageBoost) : BaseModEntity
	, IAdjustDamageToInvaders_FromSpiritPowers
	, IEndWhenTimePasses // uses RoundScope to not run again for this round, then auto remove at end of round.
{
	Task IAdjustDamageToInvaders_FromSpiritPowers.ModifyDamage(DamageFromSpiritPowers args) {

		if( spirit.ActionIsMyPower && !UsedThisRound ) {
			var damagedSpaces = SpacesDamagedThisAction;
			if(damagedSpaces.Count() == 0 )
				ActionScope.Current.AtEndOfThisAction(ApplyExtraDamage);
			damagedSpaces.Add(args.Space);
		}

		return Task.CompletedTask;
	}

	async Task ApplyExtraDamage(ActionScope scope) {

		List<Space> spaces = SpacesDamagedThisAction;
		string spaceDesc = spaces.Select(s=>s.Label).Join(", ");

		var spirit = scope.Owner!;
		if( spaces.Any(s=>s.HasInvaders())
			&& await spirit.UserSelectsFirstText($"Apply 3 additional Damage in {spaceDesc}?", $"Yes boost it by {damageBoost}", "No thank you")
		) {
			UsedThisRound = true;

			await new SourceSelector(spaces)
				.UseQuota(new DamageQuota_NoMods(3, Human.Invader))
				.DoDamageAsync(spirit, 3, Present.Always);
		}

	}

	/// <summary> Round-Scoped </summary>
	bool UsedThisRound { 
		get => GameState.Current.RoundScope.ContainsKey(_key);
		set => GameState.Current.RoundScope[_key] = true; 
	}

	/// <summary> Action-Scoped spaces that received damage. </summary>
	List<Space> SpacesDamagedThisAction {
		get { 
			var scope = ActionScope.Current;
			if( scope.ContainsKey(_key) ) return (List < Space > )scope[_key];
			var list = new List<Space>();
			scope[_key] = list;
			return list;
		}
	}

	readonly string _key = "Damage-Boost-" + Guid.NewGuid().ToString();


}
