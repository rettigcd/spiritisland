namespace SpiritIsland.JaggedEarth;

class GatherPowerFromTheCoolAndDark(Spirit s) : DrawCardStrategy(s), ISpiritMod, ICleanupSpiritWhenTimePasses {

	public const string Name = "Gather Power from the Cool and Dark";
	const string Description = "Once a turn, when you Gain a Power Card without fire, gain 1 Energy";
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	protected override async Task<DrawCardResult> Inner(PowerCardDeck deck, int numberToDraw, int numberToKeep, bool forgetACard) {
		var result = await base.Inner(deck, numberToDraw, numberToKeep, forgetACard);
		CheckForCoolEnergy(result.Selected);
		return result;
	}

	// ! This is just an Event - could switch this to Event driven instead of inheritance driven.
	void CheckForCoolEnergy(PowerCard card) {
		if(_usedThisRound) return;
		if( card.Elements[Element.Fire] > 0 ) return;
		_spirit.Energy++;
		_usedThisRound = true;
	}

	bool _usedThisRound;

	void ICleanupSpiritWhenTimePasses.CleanupSpirit( Spirit spirit ) => _usedThisRound = false;


}
