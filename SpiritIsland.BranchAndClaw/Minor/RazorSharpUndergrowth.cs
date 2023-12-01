namespace SpiritIsland.BranchAndClaw;

public class RazorSharpUndergrowth {

	[MinorCard( "Razor-Sharp Undergrowth", 1, Element.Moon, Element.Plant ),Fast,FromPresence( 0, Filter.NoBlight )]
	[Instructions( "Destroy 1 Explorer and 1 Dahan. Add 1 Wilds. Defend 2." ), Artist( Artists.CariCorene )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// destroy 1 explorer
		await ctx.Invaders.DestroyNOfClass(1,Human.Explorer);
		// and 1 dahan
		await ctx.Dahan.Destroy( 1 );
		// add 1 wilds
		await ctx.Wilds.AddAsync(1);
		// defend 2
		ctx.Defend(2);

	}

}