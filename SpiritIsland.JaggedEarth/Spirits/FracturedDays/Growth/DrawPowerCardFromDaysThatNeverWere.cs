namespace SpiritIsland.JaggedEarth;

class DrawPowerCardFromDaysThatNeverWere : SpiritAction {

	public DrawPowerCardFromDaysThatNeverWere() : base( "Draw Power Card from Days That Never Were" ) { }

	public override async Task ActAsync( Spirit self ) {
		if( self is not FracturedDaysSplitTheSky fracturedDays ) return;

		var minor = fracturedDays.DtnwMinor;
		var major = fracturedDays.DtnwMajor;

		PowerCard card = await self.SelectPowerCard("Gain Power Card from Days That Never Were", 1, minor.Union( major ), CardUse.AddToHand, Present.Always );
		if(card == null) return; // in case both hands are empty.

		fracturedDays.AddCardToHand( card );
		if(!minor.Remove( card )) {
			major.Remove( card );
			await fracturedDays.ForgetACard();
		}

	}

}