namespace SpiritIsland.Basegame;

public class SapTheStrengthOfMultitudes {

	[MinorCard( "Sap the Strength of Multitudes", 0, "water, animal" ),Fast,ExtendableRange(TargetFrom.Presence,0,"1 air",1 )]
	[Instructions( "Defend 5. -If you have- 1 Air: Increase this Power's Range by 1" ), Artist( Artists.LoicBelliau )]
	static public Task ActAsync( TargetSpaceCtx ctx) {
		// defend 5
		ctx.Defend(5);
		return Task.CompletedTask;
	}

}