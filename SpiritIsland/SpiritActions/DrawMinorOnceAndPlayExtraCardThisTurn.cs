namespace SpiritIsland.JaggedEarth;

public class DrawMinorOnceAndPlayExtraCardThisTurn : SpiritAction {

	public DrawMinorOnceAndPlayExtraCardThisTurn():base( "DrawMinorOnceAndPlayExtraCardThisTurn" ) { }


	bool drewMinor = false;

	public override async Task ActAsync( Spirit self ) {

		if(!drewMinor)
			await self.DrawMinor();
		drewMinor = true;

		self.TempCardPlayBoost++;
	}

}