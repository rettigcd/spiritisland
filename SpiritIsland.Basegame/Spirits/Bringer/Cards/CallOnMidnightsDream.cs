namespace SpiritIsland.Basegame;

public class CallOnMidnightsDream {

	public const string Name = "Call on Midnight's Dream";


	[SpiritCard(CallOnMidnightsDream.Name,0, Element.Moon,Element.Animal), Fast, FromPresence(0,Target.Dahan, Target.Invaders )]
	static public Task ActAsync(TargetSpaceCtx ctx) {

		return ctx.SelectActionOption(
			new SpaceAction("Draw Major Power", DrawMajorOrGetEnergy ).Matches( x => x.Dahan.Any), // if target land has dahan
			new SpaceAction("2 fear", ctx => ctx.AddFear(2) ).Matches( x => x.HasInvaders )
		);

	}

	static async Task DrawMajorOrGetEnergy( TargetSpaceCtx ctx ) {
		// Gain a major power. (and forget a card)
		var major = (await ctx.DrawMajor(true)).Selected;

		// If you Forget this (Call on Midnights Dream) Power,
		if( !ctx.Self.Hand.Any(c=>c.Name ==CallOnMidnightsDream.Name ) ) {
			// gain energy equal to dahan
			ctx.Self.Energy += ctx.Dahan.Count;
			// you may play the major power immediately by paying its cost
			if( major.Cost <= ctx.Self.Energy 
				&& await ctx.Self.UserSelectsFirstText("Play card immediately by paying its cost?", "Yes, play now.", "No thank you")
			)
				ctx.Self.PlayCard( major );
		}
	}

}