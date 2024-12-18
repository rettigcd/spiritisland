using System;

namespace SpiritIsland.JaggedEarth;

public class UnleashATorrentOfTheSelfsOwnEssence {

	[MajorCard("Unleash a Torrent of the Self's Own Essence",2, Element.Sun,Element.Moon,Element.Fire,Element.Water), Fast, Yourself]
	[Instructions( "Gain 4 energy. You may forget a power card to gain 4 more energy. -or- Pay X energy (min. 1) to deal X damage in a land at 0 Range.  -If you have- 2 Sun, 3 Fire: You may do both." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync( Spirit self ) {

		await Cmd.Pick1(
			GainEnergy,
			ConvertEnergyToDamage,
			// if you have 2 sun,3 fire: You may do both.
			Cmd.Multiple( "Do Both", GainEnergy, ConvertEnergyToDamage )
				.OnlyExecuteIf( await self.Elements.YouHave( "2 sun,3 fire" ) )
		).ActAsync(self);

	}

	static SpiritAction GainEnergy => new SpiritAction("Gain 4 energy, Forget a Power Card to gain 4 more", GainEnergyMethod);

	static async Task GainEnergyMethod( Spirit self ) {
		// Gain 4 energy.
		self.Energy += 4;

		// you may forget a Power Card to gain 4 more Energy
		var card = await self.Forget.ACard(null,Present.Done);
		if(card != null )
			self.Energy += 4;
	}

	static SpiritAction ConvertEnergyToDamage => new SpiritAction(
		"Pay X Energy (min 1) to deal X Damage in a land at range 0", 
		ConvertEnergyToDamageMethod
	);

	static async Task ConvertEnergyToDamageMethod( Spirit self ) {
		// Pay X Energy (min. 1) to deal X Damage
		int damage = await self.SelectNumber("Pay 1 energy/damage.", self.Energy, 0);
		if(damage == 0) return;

		//  in a land at range-0
		var options = self.FindSpacesWithinRange(new TargetCriteria(0));
		var land = await self.SelectAsync( new A.SpaceDecision( $"{damage} Damage", options,Present.Always ) );
		if(land is null) return;

		self.Energy -= damage;
		await self.Target(land).DamageInvaders(damage);
	}

}