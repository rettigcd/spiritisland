namespace SpiritIsland.JaggedEarth;
public class ShapeTheSelfAnew {

	public const string Name = "Shape the Self Anew";

	[SpiritCard( ShapeTheSelfAnew.Name, 0, Element.Moon), Slow, Yourself]
	[Instructions( "Gain a Minor Power. You may Forget this Power Card to gain 3 Energy.  -If you have- 4 Moon: You may gain a Major Power instead of a Minor Power." ), Artist( Artists.EmilyHancock )]
	public static async Task ActAsync( Spirit self ) {
		// Gain a Minor Power.
		// If you have 4 moon, You may gain a Major Power instead of a Minor Power.
		if(await self.YouHave("4 moon"))
			await self.Draw();
		else
			await self.DrawMinor();

		// You may Forget this Power Card to gain 3 Energy.
		var thisCard = self.InPlay.SingleOrDefault( x => x.Title == ShapeTheSelfAnew.Name );
		if( thisCard != null // might have already been forgotten when picking a major card.
			&& await self.UserSelectsFirstText( $"Forget '{Name} for +3 energy.", "Yes, forget it.", "no thanks." )
		) {
			// Forget this Power Card
			self.ForgetThisCard( thisCard );

			// gain 3 energy
			self.Energy += 3;
		}

	}

}