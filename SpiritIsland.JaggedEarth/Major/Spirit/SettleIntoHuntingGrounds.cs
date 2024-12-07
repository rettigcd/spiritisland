namespace SpiritIsland.JaggedEarth;

public class SettleIntoHuntingGrounds {

	const string Name = "Settle Into Hunting-Grounds";

	const string TokenBadge = "😀";

	[MajorCard(Name,3,Element.Moon,Element.Fire,Element.Plant,Element.Animal), Fast, Yourself]
	[Instructions( "Your Presence may count as Badlands and Beasts. (Decide per Presence, per action.) Your Presence cannot move. -If you have- 2 Plant, 3 Animal: 2 Fear and 2 Damage in one of your lands." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync( Spirit self ) {

		// Your presence may count as badlands and beast.
		var presenceBeast = new TokenVariety(Token.Beast, TokenBadge);
		var presenceBadland = new TokenVariety(Token.Badlands, TokenBadge);

		// !!! use these: public class TokenClassToken : IToken, IAppearInSpaceAbreviation
		// var presenceBeast = new VarietyToken("Beast", 'A', Img.Beast, Token.Beast, "😀");
		// var presenceBadland = new VarietyToken("Badlands", 'M', Img.Badlands, Token.Badlands "😀");

		foreach( var land in self.Presence.Lands ) {
			int count = self.Presence.CountOn(land);
			land.Adjust(presenceBeast, count);
			land.Adjust(presenceBadland, count);
		}
		// your presence cannot move.
		GameState.Current.Tokens.AddIslandMod( new FreezePresence( Name, self.Presence, presenceBeast, presenceBadland));

		// (Decide per presence, per action) ... Not doing this bit exactly, both are always present, but can't be destroyed.


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

class FreezePresence( string _name, SpiritPresence _presence, IToken beast, IToken badland ) : BaseModEntity 
	,IModifyRemovingToken // Prevent presence from being moved.
	,IRunWhenTimePasses
{
	bool IRunWhenTimePasses.RemoveAfterRun => true;
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;
	Task IRunWhenTimePasses.TimePasses(GameState gameState) {

		foreach(var land in gameState.Spaces_Unfiltered ) {
			land.Init(beast, 0);
			land.Init(badland, 0 );
		}

		return Task.CompletedTask;
	}


	Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {
		if( args.Reason.IsOneOf(RemoveReason.MovedFrom, RemoveReason.Abducted) && this.IsFrozen(args.Token) ) {
			ActionScope.Current.Log(new Log.Debug($"{_name} prevented {args.Token.Text} from moving from {args.From.Label}"));
			args.Count = 0;
		}
		return Task.CompletedTask;
	}

	bool IsFrozen(IToken token) => token == badland || token == beast || token.HasTag(_presence);
}

