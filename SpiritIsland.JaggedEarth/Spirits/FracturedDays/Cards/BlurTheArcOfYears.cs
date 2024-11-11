namespace SpiritIsland.JaggedEarth;

class BlurTheArcOfYears {

	const string Name = "Blur the Arc of Years";

	[SpiritCard( BlurTheArcOfYears.Name, 1, Element.Sun, Element.Moon,Element.Air ), Fast, FromPresence( 1 )]
	[Instructions( "If no Dahan / Invaders are present: Remove 1 Blight. If invaders are present: they Build, then Ravage. If Dahan are present: Add 1 Dahan. Push up to 2 Dahan. You may repeat this power (once) on the same land by spending 1 Time." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		await ActInnerAsync( ctx );

		// You may repeat this Power (once) on the same land by spending 1 Time.
		if(ctx.Self is FracturedDaysSplitTheSky frac
			&& frac.Time > 0
			&& await frac.UserSelectsFirstText($"Pay 1 Time to repeat '{BlurTheArcOfYears.Name}' on {ctx.SpaceSpec.Label}?", "Yes", "No, thank you" )
		) {
			await frac.SpendTime( 1 );
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
			var deck = GameState.Current.InvaderDeck;
			await deck.Build.Engine.TryToDo1Build( ctx.Space );
			await ctx.Space.Ravage();
		}

		// If dahan are present: Add 1 dahan. Push up to 2 dahan.
		if(hasDahan) {
			await ctx.Dahan.AddDefault( 1 );
			await ctx.PushUpToNDahan( 2 );
		}
	}

}