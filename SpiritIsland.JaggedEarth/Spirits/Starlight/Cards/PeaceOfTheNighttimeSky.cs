namespace SpiritIsland.JaggedEarth;

public class PeaceOfTheNighttimeSky {

	public const string Name = "Peace of the Nighttime Sky";

	[SpiritCard( PeaceOfTheNighttimeSky.Name,1,Element.Moon), Fast, FromSacredSite(1)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// If the Terror Level is 1
		bool canStopRavage = ctx.GameState.Fear.TerrorLevel == 1;
		if(canStopRavage )
			// Invaders do not Ravage in target land this turn.
			ctx.GameState.SkipRavage( ctx.Space ); // 

		// You may Repeat this Power.  If you do, 
		if(await ctx.Self.UserSelectsFirstText("Repeat Power,Forget Card,+1 moon","Yes, forget it", "no thanks." )) {
			// Forget this Power Card
			var thisCard = ctx.Self.InPlay.Single(x=>x.Name == PeaceOfTheNighttimeSky.Name );
			ctx.Self.Forget(thisCard);

			//  Gain 1 Moon.
			ctx.Self.Elements[Element.Moon]++;

			var secondSpace = await ctx.Self.TargetsSpace(TargetingPowerType.PowerCard, ctx.GameState,"Stop Ravage"
				,new TargetSourceCriteria( From.SacredSite )
				,new TargetCriteria(1,Target.Any)
			);
			if( canStopRavage )
				// Invaders do not Ravage in target land this turn.
				ctx.GameState.SkipRavage( secondSpace );

		}
	}

}