namespace SpiritIsland.JaggedEarth;
public class ShapeTheSelfAnew {

	public const string Name = "Shape the Self Anew";

	[SpiritCard( ShapeTheSelfAnew.Name, 0, Element.Moon), Slow, Yourself]
	public static async Task ActAsync( SelfCtx ctx ) {
		// Gain a Minor Power.
		// If you have 4 moon, You may gain a Major Power instead of a Minor Power.
		if(await ctx.YouHave("4 moon"))
			await ctx.Draw();
		else
			await ctx.DrawMinor();

		// You may Forget this Power Card to gain 3 Energy.
		var thisCard = ctx.Self.InPlay.SingleOrDefault( x => x.Name == ShapeTheSelfAnew.Name );
		if( thisCard != null // might have already been forgotten when picking a major card.
			&& await ctx.Self.UserSelectsFirstText( $"Forget '{Name} for +3 energy.", "Yes, forget it.", "no thanks." )
		) {
			// Forget this Power Card
			ctx.Self.Forget( thisCard );

			// gain 3 energy
			ctx.Self.Energy += 3;
		}

	}

}