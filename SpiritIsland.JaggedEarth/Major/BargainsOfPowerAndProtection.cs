
namespace SpiritIsland.JaggedEarth;

public class BargainsOfPowerAndProtection {

	[MajorCard("Bargains of Power and Protection",2,Element.Sun,Element.Water,Element.Earth,Element.Animal), Fast, FromPresence(0,Filter.Dahan)]
	[Instructions( "Remove 1 of your Presence on the island from the game, setting it on the Reminder Card. From now on: each Dahan within 1 Range of your Presence provides Defend 1 in its land, and you gain 1 less Energy each turn. (This effect stacks if used multiple times) -If you have- 3 Sun, 2 Water, 2 Earth: The Presence instead comes from your Presence track." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync( TargetSpaceCtx ctx ) {
		// Remove 1 of your presence on the island from the game, setting it on the Reminder Card.
		// if you have 3 sun 2 water 2 earth: the presence instead comes from your presence track.
		await ctx.Self.PayPresenceForBargain( "3 sun,2 water,2 earth" );

		// From now on: Each dahan within range of 1 of your presence provides Defend 1 in its land
		GameState.Current.AddIslandMod(new EachDahanAtRange1Defend1(ctx.Self));
	
		// and you gain 1 less Energy each turn.
		ctx.Self.Presence.AdjustEnergyTrackDueToBargain(-1);

		// (this effect stacks if used multiple times.)
	}

	// From now on: Each dahan within range of 1 of your presence provides Defend 1 in its land
	class EachDahanAtRange1Defend1( Spirit self )
		: BaseModEntity
		, IHandleTokenAdded, IHandleTokenRemoved {

		Task IHandleTokenAdded.HandleTokenAddedAsync(Space to, ITokenAddedArgs args) {
			if( IsPresence(args.Added) ) Presence_Added(args);
			if( IsDahan(args.Added) ) Dahan_Added(args);
			return Task.CompletedTask;
		}

		Task IHandleTokenRemoved.HandleTokenRemovedAsync(ITokenRemovedArgs args) {
			if( IsPresence( args.Removed ) ) Presence_Removed(args);
			if( IsDahan( args.Removed ) ) Dahan_Removed(args);
			return Task.CompletedTask;
		}

		void Presence_Added(ITokenAddedArgs args) {
			if( args.TokenIsNew() && args.To is Space space ) // new presence
				foreach( var other in space.Range(1) )
					Update(other);
		}

		void Presence_Removed(ITokenRemovedArgs args) {
			if( args.From is not Space space ) return;
			if( args.TokenIsRetired() )
				foreach( var other in space.Range(1) )
					if( other.Dahan.Any && !HasPresenceWithinRange(other) )
						other.Init(_myDefend, 0);
		}

		void Dahan_Added(ITokenAddedArgs args) {
			if( args.To is not Space space ) return;
			if( space[_myDefend] > 0    // previously defended
				|| args.TokenIsNew() && HasPresenceWithinRange(space) // has presence within 1
			) Update(space);
		}

		void Dahan_Removed(ITokenRemovedArgs args) {
			if( args.From is Space space && space[_myDefend] > 0 )
				Update(space);
		}

		bool IsPresence(IToken token) => token == self.Presence.Token;
		bool IsDahan(IToken token) => token.HasTag(Human.Dahan);
		bool HasPresenceWithinRange(Space space) => space.Range(1).Any(self.Presence.IsOn);

		void Update(Space space) {
			space.Init(_myDefend, space.Dahan.CountAll);
		}

		readonly TokenVariety _myDefend = new TokenVariety(Token.Defend, "💪");
	}


}