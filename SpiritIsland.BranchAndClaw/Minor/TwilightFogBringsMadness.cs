namespace SpiritIsland.BranchAndClaw;

public class TwilightFogBringsMadness {

	[MinorCard( "Twilight Fog Brings Madness", 0, Element.Sun, Element.Moon, Element.Air, Element.Water ), Slow, FromPresence( 1 )]
	[Instructions( "Add 1 Strife. Push 1 Dahan. Each remaining Dahan takes 1 Damage." ), Artist( Artists.LoicBelliau )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Add 1 strife
		await ctx.AddStrife();

		// Push 1 dahan
		await ctx.PushDahan( 1 );

		// Each remaining Dahan take 1 damage
		await ctx.Apply1DamageToEachDahan();

	}

}