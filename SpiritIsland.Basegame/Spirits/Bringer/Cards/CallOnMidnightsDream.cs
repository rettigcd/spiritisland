namespace SpiritIsland.Basegame;

public class CallOnMidnightsDream {

	public const string Name = "Call on Midnight's Dream";


	[SpiritCard(Name,0, Element.Moon,Element.Animal), Fast, FromPresence(0,Filter.Dahan, Filter.Invaders )]
	[Instructions("If target land has Dahan, gain a Major Power. If you Forget this Power, gain Energy equal to Dahan and you may play the Major Power immediately, paying its cost. -or- If Invaders are present, 2 Fear."),Artist( Artists.ShaneTyree)]
	static public Task ActAsync(TargetSpaceCtx ctx) {

		return ctx.SelectActionOption(
			new SpaceAction("Draw Major Power", DrawMajorOrGetEnergy ).OnlyExecuteIf( x => x.Dahan.Any), // if target land has dahan
			new SpaceAction("2 fear", ctx => ctx.AddFear(2) ).OnlyExecuteIf( x => x.HasInvaders )
		);

	}

	static async Task DrawMajorOrGetEnergy( TargetSpaceCtx ctx ) {
		// Gain a major power. (and forget a card)
		var major = (await ctx.Self.DrawMajor(true)).Selected;

		// If you Forget this (Call on Midnights Dream) Power,
		var callOnMidnightsDreamCard = ctx.Self.InPlay.SingleOrDefault( x => x.Name == Name );
		string prompt = major.Cost <= ctx.Self.Energy + ctx.Dahan.CountAll 
			? $"Forget '{Name} for +{ctx.Dahan.CountAll} energy and option to play {major.Name}."
			: $"Forget '{Name} for +{ctx.Dahan.CountAll} energy.";
		if(callOnMidnightsDreamCard != null // might have already been forgotten when picking a major card.
			&& await ctx.Self.UserSelectsFirstText( prompt, "Yes, forget it.", "no thanks." )
		) {
			// Forget Call On Midnight's dream
			ctx.Self.ForgetThisCard( callOnMidnightsDreamCard );

			// gain energy equal to dahan
			ctx.Self.Energy += ctx.Dahan.CountAll;

			if( major.Cost <= ctx.Self.Energy
				&& await ctx.Self.UserSelectsFirstText( $"Pay {major.Cost} to play {major.Name} immediately?", "Yes play it","no thanks" ) 
			)
				ctx.Self.PlayCard( major );

		}

	}

}