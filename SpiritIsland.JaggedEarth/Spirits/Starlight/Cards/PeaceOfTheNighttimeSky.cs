namespace SpiritIsland.JaggedEarth;

public class PeaceOfTheNighttimeSky {

	public const string Name = "Peace of the Nighttime Sky";

	[SpiritCard( PeaceOfTheNighttimeSky.Name,1,Element.Moon), Fast, FromSacredSite(1), RepeatForPeace]
	public static Task ActAsync(TargetSpaceCtx ctx ) {
		// If the Terror Level is 1
		if(ctx.GameState.Fear.TerrorLevel == 1)
			// Invaders do not Ravage in target land this turn.
			ctx.GameState.SkipRavage( ctx.Space );
		return Task.CompletedTask;
	}

}

public class RepeatForPeace : RepeatAttribute {

	public override IDrawableInnateOption[] Thresholds => Array.Empty<IDrawableInnateOption>();

	public override IPowerRepeater GetRepeater() => new Repeater();

	class Repeater : IPowerRepeater {

		bool repeated = false;

		public async Task<bool> ShouldRepeat( Spirit self ) {
			if( repeated
				|| !await self.UserSelectsFirstText( "Repeat Power,Forget Card,+1 moon", "Yes, forget it", "no thanks." )
			) return false;

			// Forget this Power Card
			var thisCard = self.InPlay.Single( x => x.Name == PeaceOfTheNighttimeSky.Name );
			self.Forget( thisCard );

			//  Gain 1 Moon.
			self.Elements[Element.Moon]++;

			repeated = true;
			return true;
		}
	}

}
