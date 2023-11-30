namespace SpiritIsland.JaggedEarth;

public class SettleIntoHuntingGrounds {

	[MajorCard("Settle Into Hunting-Grounds",3,Element.Moon,Element.Fire,Element.Plant,Element.Animal), Fast, Yourself]
	[Instructions( "Your Presence may count as Badlands and Beasts. (Decide per Presence, per action.) Your Presence cannot move. -If you have- 2 Plant, 3 Animal: 2 Fear and 2 Damage in one of your lands." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync(SelfCtx ctx ) {

		// Your presence may count as badlands and beast.
		var gs = GameState.Current;
		gs.Tokens.Dynamic.ForRound.Register( ctx.Self.Presence.CountOn, Token.Badlands );
		gs.Tokens.Dynamic.ForRound.Register( ctx.Self.Presence.CountOn, Token.Beast );
		// (Decide per presence, per action) ... Not doing this bit exactly, both are always present, but can't be destroyed.

		// your presence cannot move.
		ctx.Self.Presence.CanMove = false;
		gs.TimePasses_ThisRound.Push( _ => {
			ctx.Self.Presence.CanMove = true; 
			return Task.CompletedTask;
		} );

		// if you have 2 plant 3 animal:
		if( await ctx.YouHave("2 plant,3 animal" )){
			// 2 fear and
			ctx.AddFear(2);
			// 2 damamge in one of your lands.
			var space = await ctx.SelectAsync( new A.Space("2 damage", ctx.Self.Presence.Lands, Present.Always ));
			if( space != null )
				await ctx.Target(space).DamageInvaders( 2 );
		}
	}

}