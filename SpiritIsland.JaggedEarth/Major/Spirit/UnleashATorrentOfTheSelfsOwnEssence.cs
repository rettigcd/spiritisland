namespace SpiritIsland.JaggedEarth;

public class UnleashATorrentOfTheSelfsOwnEssence {

	[MajorCard("Unleash a Torrent of the Self's Own Essence",2, Element.Sun,Element.Moon,Element.Fire,Element.Water), Fast, Yourself]
	public static async Task ActAsync(SelfCtx ctx ) {

		await ctx.SelectActionOption(
			GainEnergy,
			ConvertEnergyToDamage,
			// if you have 2 sun,3 fire: You may do both.
			Cmd.Multiple( "Do Both", GainEnergy, ConvertEnergyToDamage )
				.FilterOption( await ctx.YouHave( "2 sun,3 fire" ) )
		);

	}

	static SelfAction GainEnergy => new SelfAction("Gain 4 energy, Forget a Power Card to gain 4 more", GainEnergyMethod);

	static async Task GainEnergyMethod(SelfCtx ctx ) {
		// Gain 4 energy.
		ctx.Self.Energy += 4;

		// you may forget a Power Card to gain 4 more Energy
		var card = await ctx.Self.ForgetPowerCard_UserChoice(Present.Done);
		if(card != null )
			ctx.Self.Energy += 4;
	}

	static SelfAction ConvertEnergyToDamage => new SelfAction("Pay X Energy (min 1) to deal X Damage in a land at range 0", ConvertEnergyToDamageMethod);

	static async Task ConvertEnergyToDamageMethod(SelfCtx ctx ) {
		// Pay X Energy (min. 1) to deal X Damage
		int damage = await ctx.Self.SelectNumber("Pay 1 energy/damage.", ctx.Self.Energy, 0);
		if(damage == 0) return;

		//  in a land at range-0
		var options = ctx.Presence.FindSpacesWithinRange( new TargetCriteria( 0 ), TargetingPowerType.PowerCard );
		var land = await ctx.Decision( new Select.Space( $"{damage} Damage", options,Present.Always ) );

		ctx.Self.Energy -= damage;
		await ctx.Target(land).DamageInvaders(damage);
	}

}