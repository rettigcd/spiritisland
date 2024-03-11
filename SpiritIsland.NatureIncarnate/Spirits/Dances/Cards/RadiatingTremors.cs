namespace SpiritIsland.NatureIncarnate;

public class RadiatingTremors {
	const string Name = "Radiating Tremors";

	[SpiritCard( Name, 2, Element.Moon, Element.Fire, Element.Earth ), Slow, FromPresence(0)]
	[Instructions( "2 Damage. You may Push any number of Quake dividing them as evenly as possible between adjacent lands." ), Artist( Artists.EmilyHancock )]
	static async public Task ActAsync( TargetSpaceCtx ctx ) {
		// 2 Damage
		await ctx.DamageInvaders(2);

		// You may Push any number of Quake
		await ctx.SourceSelector
			.AddGroup( ctx.Space[Token.Quake], Token.Quake )
			// dividing them as evenly as possible between adjacent lands.
			.ConfigDestination( Distribute.Evenly )
			.PushUpToN(ctx.Self);
	}

}
