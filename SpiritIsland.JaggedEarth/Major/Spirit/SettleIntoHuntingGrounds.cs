namespace SpiritIsland.JaggedEarth;

public class SettleIntoHuntingGrounds {

	[MajorCard("Settle Into Hunting-Grounds",3,Element.Moon,Element.Fire,Element.Plant,Element.Animal), Fast, Yourself]
	[Instructions( "Your Presence may count as Badlands and Beasts. (Decide per Presence, per action.) Your Presence cannot move. -If you have- 2 Plant, 3 Animal: 2 Fear and 2 Damage in one of your lands." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync( Spirit self ) {

		// Your presence may count as badlands and beast.
		var gs = GameState.Current;
		gs.Tokens.Dynamic.ForRound.Register( self.Presence.CountOn, Token.Badlands );
		gs.Tokens.Dynamic.ForRound.Register( self.Presence.CountOn, Token.Beast );
		// (Decide per presence, per action) ... Not doing this bit exactly, both are always present, but can't be destroyed.

		// your presence cannot move.
		self.Presence.CanMove = false;
		gs.TimePasses_ThisRound.Push( _ => {
			self.Presence.CanMove = true; 
			return Task.CompletedTask;
		} );

		// if you have 2 plant 3 animal:
		if( await self.YouHave("2 plant,3 animal" )){
			// 2 fear and
			self.AddFear(2);
			// 2 damamge in one of your lands.
			var space = await self.SelectAsync( new A.Space("2 damage", self.Presence.Lands, Present.Always ));
			if( space != null )
				await self.Target(space).DamageInvaders( 2 );
		}
	}

}