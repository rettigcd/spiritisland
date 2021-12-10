using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	class BlurTheArcOfYears {

		const string Name = "Blur the Arc of Years";

		[SpiritCard( BlurTheArcOfYears.Name, 1, Element.Sun, Element.Moon,Element.Air ), Fast, FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ActInnerAsync( ctx );

			// You may repeat this Power (once) on the same land by spending 1 Time.
			if(ctx.Self is FracturedDaysSplitTheSky frac
				&& frac.Time > 0
				&& await frac.UserSelectsFirstText($"Pay 1 Time to repeat '{BlurTheArcOfYears.Name}' on {ctx.Space.Label}?", "Yes", "No, thank you" )
			) {
				--frac.Time;
				await ActInnerAsync( ctx );
			}

		}

		static async Task ActInnerAsync( TargetSpaceCtx ctx ) {
			bool hasDahan = ctx.Dahan.Any;
			bool hasInvaders = ctx.HasInvaders;

			// If no dahan / Invaders are present: Remove 1 blight.
			if(!hasDahan && !hasInvaders)
				await ctx.RemoveBlight();

			// If invaders are present: they Build, then Ravage
			if(hasInvaders) {
				await ctx.GameState.InvaderEngine.BuildSpace( ctx.Tokens, BuildingEventArgs.BuildType.TownsAndCities );
				await ctx.GameState.InvaderEngine.RavageSpace( ctx.Invaders );
			}

			// If dahan are present: Add 1 dahan. Push up to 2 dahan.
			if(hasDahan) {
				ctx.Dahan.Add( 1 );
				await ctx.PushUpToNDahan( 2 );
			}
		}
	}

}
