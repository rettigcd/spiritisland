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
	async Task IAdjustDamageToInvaders_FromSpiritPowers.ModifyDamage(DamageFromSpiritPowers args) {
		bool boostIt = spirit.ActionIsMyPower
			&& !Used
			&& await spirit.UserSelectsFirstText($"Boost damage from {args.Damage} to {args.Damage + damageBoost}?", $"Yes boost it by {damageBoost}", "No thank you");
		if( boostIt ) {
			args.Damage += damageBoost;
			Used = true;
		}
	}

	readonly string _key = "Damage-Boost-"+Guid.NewGuid().ToString();
	bool Used { 
		get => GameState.Current.RoundScope.ContainsKey(_key);
		set => GameState.Current.RoundScope[_key] = true; 
	}

}
