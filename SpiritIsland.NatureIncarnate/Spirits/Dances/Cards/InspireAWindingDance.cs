namespace SpiritIsland.NatureIncarnate;

public class InspireAWindingDance {
	const string Name = "Inspire a Winding Dance";

	[SpiritCard( Name, 0, Element.Moon, Element.Water, Element.Earth, Element.Animal ), Slow, FromPresence( 1, Target.Invaders )]
	[Instructions( "Push up to 1 Explorer/Town. Gather up to 1 Dahan." ), Artist( Artists.EmilyHancock )]
	static async public Task ActAsync( TargetSpaceCtx ctx ) {
		// Push up to 1 Explorer / Town.
		await ctx.PushUpTo(1, Human.Explorer_Town);
		// Gather up to 1 Dahan.
		await ctx.GatherUpToNDahan(1);
	}

}

