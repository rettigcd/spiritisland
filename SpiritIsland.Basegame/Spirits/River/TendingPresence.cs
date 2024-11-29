namespace SpiritIsland.Basegame;

public class TendingPresence( Spirit spirit, IPresenceTrack t1, IPresenceTrack t2 )
	: SpiritPresence( spirit, t1, t2, new TendingPresenceToken(spirit) )
{

	static public SpecialRule Rule => new SpecialRule(
		"People Tend to the River, River Tends to the People",
		"Your lands with 4 or more Dahan are considered Sacred Sites, and have Defend 1."
	);

	// - are considered Sacred Sites
	public override bool IsSacredSite(Space space) => base.IsSacredSite(space)
		|| ((TendingPresenceToken)Token).IsTended(space);

	class TendingPresenceToken(Spirit spirit) : SpiritPresenceToken(spirit), IHandleTokenAdded {

		// Your lans with 4 or more Dahan....
		public bool IsTended(Space space) => 1 <= space[this]
			&& 4 <= space.Sum(TokenCategory.Dahan);

		void IHandleTokenAdded.HandleTokenAdded(Space to, ITokenAddedArgs args) {
			// - Defend 1 - (Registers the dynamic defend the first time the token is added to a space)
			if( _registered ) return;
			GameState.Current.Tokens.Dynamic.ForGame.Register(s => IsTended(s) ? 1 : 0, SpiritIsland.Token.Defend);
			_registered = true;
		}

		bool _registered = false;
	}

}
