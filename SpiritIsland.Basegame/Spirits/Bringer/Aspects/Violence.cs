using System.Text.RegularExpressions;

namespace SpiritIsland.Basegame;

public partial class Violence : IAspect {

	// https://spiritislandwiki.com/index.php?title=Violence

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Bringer.Name, Name);
	public const string Name = "Violence";
	public string[] Replaces => [];

	public void ModSpirit(Spirit spirit) {
		// Gain 1 Energy
		spirit.Energy++;

		// Card plays double for Damage cards
		spirit.Presence.Energy.Slots.First().Action = new SpiritAction("Play Damage Cards", SelectAndPlayDamageCardsFromHand);

		// Replace Dreams of the Dahan with Bats Scout for Raids by Darkness.
		spirit.ReplaceCard(DreamsOfTheDahan.Name, PowerCard.ForDecorated(BatsScoutForRaidsByDarkness));

		// "When To Dream a Thousand Deaths generates Fear, generate +1 Fear per affected Explorer/Town (to 1/3/5 Fear for Explorers/Towns/Cities, respectively)."
		TDaTD_ActionTokens.DreamFear[0] = 1;
		TDaTD_ActionTokens.DreamFear[1] = 3;

		spirit.SpecialRules = [..spirit.SpecialRules, Rule];
	}

	static SpecialRule Rule => new SpecialRule(
		"Nightmares of Violence and Death",
		"Card Plays on your bottom Presence track grant twice as many Plays during the Spirit Phase. These extra Card Plays can only be used for Power Cards with Damage or Destroy instructions. " +
		"When To Dream a Thousand Deaths generates Fear, generate +1 Fear per affected Explorer/Town (to 1/3/5 Fear for Explorers/Towns/Cities, respectively)."
	);

	#region Bats Scout For Raids by Darkness

	// Repeated here so we don't have a dependency on Jagged Earth.

	[MinorCard("Bats Scout for Raids by Darkness", 1, Element.Moon, Element.Air, Element.Animal), Slow, FromPresence(2)]
	[Instructions("For each Dahan, 1 Damage to Town / City. -or- 1 Fear. Gather up to 2 Dahan."), Artist(Artists.ShawnDaley)]
	static Task BatsScoutForRaidsByDarkness(TargetSpaceCtx ctx) {
		return ctx.SelectActionOption(
			new SpaceAction("For each dahan, 1 Damage to town/city.", EachDahanDamagesTownOrCity),
			new SpaceAction("1 Fear. Gather up to 2 Dahan", OneFearAndGatherUpTo2Dahan)
		);
	}

	static Task EachDahanDamagesTownOrCity(TargetSpaceCtx ctx) => ctx.DamageInvaders(ctx.Dahan.CountAll, Human.Town_City);

	static async Task OneFearAndGatherUpTo2Dahan(TargetSpaceCtx ctx) {
		await ctx.AddFear(1);
		await ctx.GatherUpToNDahan(2);
	}

	#endregion Bats Scout For Raids by Darkness


	#region Play Damage Cards

	// Copied out of Spirit
	async Task SelectAndPlayDamageCardsFromHand(Spirit spirit) {
		PowerCard[] powerCardOptions;
		int remainingToPlay = spirit.NumberOfCardsPlayablePerTurn;
		while( 0 < remainingToPlay
			&& 0 < (powerCardOptions = GetPowerCardOptions(spirit)).Length
			&& await SelectAndPlay1(spirit,powerCardOptions, remainingToPlay)
		)
			--remainingToPlay;
	}

	static PowerCard[] GetPowerCardOptions(Spirit spirit) => [.. spirit.Hand.Where(c => c.Cost <= spirit.Energy && IsDamageCard(c))];

	static bool IsDamageCard(PowerCard card) => DamageCardInstructions().IsMatch(card.Instructions);

	// Copied out of Spirit
	static async Task<bool> SelectAndPlay1(Spirit spirit, PowerCard[] powerCardOptions, int remainingToPlay) {
		string prompt = $"Play DAMAGE power card (${spirit.Energy} / {remainingToPlay})";
		PowerCard? card = await spirit.SelectPowerCard(prompt, remainingToPlay, powerCardOptions, CardUse.Play, Present.Done);
		if( card is null ) return false;

		spirit.PlayCard(card);

		foreach( var mod in spirit.Mods.OfType<IHandleCardPlayed>() )
			await mod.Handle(spirit, card);

		return true;
	}

	[GeneratedRegex(@"(damage|destroy)\b", RegexOptions.IgnoreCase, "en-US")]
	private static partial Regex DamageCardInstructions();

	#endregion Play Damage Cards


}
