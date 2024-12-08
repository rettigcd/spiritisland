namespace SpiritIsland.JaggedEarth;

public class PeaceOfTheNighttimeSky {

	public const string Name = "Peace of the Nighttime Sky";

	[SpiritCard( PeaceOfTheNighttimeSky.Name,1,Element.Moon), Fast, FromSacredSite(1), RepeatThenForgetPower]
	[Instructions( "If the Terror Level is 1, Invaders do not Ravage in target land this turn. You may Repeat this Power. If you do, Forget this Power Card and Gain 1 Moon." ), Artist( Artists.EmilyHancock )]
	public static Task ActAsync(TargetSpaceCtx ctx ) {
		// If the Terror Level is 1
		if(GameState.Current.Fear.TerrorLevel == 1)
			// Invaders do not Ravage in target land this turn.
			ctx.Space.SkipRavage( Name );
		return Task.CompletedTask;
	}

}

/// <summary>
/// Custom repeater for "Peace of the Nighttime Sky" that causes user to lose the power.
/// </summary>
public class RepeatThenForgetPower : RepeatAttribute {

	public override IDrawableInnateTier[] ThresholdTiers => [];

	public override IPowerRepeater GetRepeater(bool isPowerCard) => new Repeater();

	class Repeater : IPowerRepeater {

		bool repeated = false;

		public async Task<bool> ShouldRepeat( Spirit self ) {
			if( repeated
				|| !await self.UserSelectsFirstText( "Repeat Power,Forget Card,+1 moon", "Yes, forget it", "no thanks." )
			) return false;

			// Forget this Power Card
			var thisCard = self.InPlay.Single( x => x.Title == PeaceOfTheNighttimeSky.Name );
			self.ForgetThisCard( thisCard );

			//  Gain 1 Moon.
			self.Elements.Add(Element.Moon);

			repeated = true;
			return true;
		}
	}

}
