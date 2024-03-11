namespace SpiritIsland.JaggedEarth;

public class SettleIntoHuntingGrounds {

	const string Name = "Settle Into Hunting-Grounds";

	[MajorCard(Name,3,Element.Moon,Element.Fire,Element.Plant,Element.Animal), Fast, Yourself]
	[Instructions( "Your Presence may count as Badlands and Beasts. (Decide per Presence, per action.) Your Presence cannot move. -If you have- 2 Plant, 3 Animal: 2 Fear and 2 Damage in one of your lands." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync( Spirit self ) {

		// Your presence may count as badlands and beast.
		var gs = GameState.Current;
		gs.Tokens.Dynamic.ForRound.Register( self.Presence.CountOn, Token.Badlands );
		gs.Tokens.Dynamic.ForRound.Register( self.Presence.CountOn, Token.Beast );
		// (Decide per presence, per action) ... Not doing this bit exactly, both are always present, but can't be destroyed.

		// your presence cannot move.
		gs.Tokens.AddIslandMod( new FreezePresence( Name, self.Presence) );

		// if you have 2 plant 3 animal:
		if( await self.YouHave("2 plant,3 animal" )){
			// 2 fear and
			self.AddFear(2);
			// 2 damamge in one of your lands.
			var space = await self.SelectAsync( new A.SpaceDecision("2 damage", self.Presence.Lands, Present.Always ));
			if( space != null )
				await self.Target(space).DamageInvaders( 2 );
		}
	}

}

class FreezePresence( string name, SpiritPresence presence ) : BaseModEntity , IModifyRemovingToken, IEndWhenTimePasses {
	readonly string _name = name;
	readonly SpiritPresence _presence = presence;

	void IModifyRemovingToken.ModifyRemoving( RemovingTokenArgs args ) {
		if(args.Token.HasTag(_presence) 
			&& args.Reason.IsOneOf(RemoveReason.MovedFrom,RemoveReason.Abducted)
        ) {
			ActionScope.Current.Log(new Log.Debug($"{_name} prevented {args.Token.Text} from moving from {args.From.Label}"));
			args.Count = 0;
		}
	}
}