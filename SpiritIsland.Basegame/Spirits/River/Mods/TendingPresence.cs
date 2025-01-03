using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SpiritIsland.Basegame;

/// <summary>
/// Makes space with 4 or more Dahan a Sacred Site
/// </summary>
public class TendingPresence( Spirit spirit, IPresenceTrack t1, IPresenceTrack t2 )
	: SpiritPresence( spirit, t1, t2, new TendingPresenceToken(spirit) )
{

	public const string Name = "People Tend to the River, River Tends to the People";
	const string Description = "Your lands with 4 or more Dahan are considered Sacred Sites, and have Defend 1.";
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	static public void InitAspect(Spirit spirit) {
		var old = spirit.Presence;
		spirit.Presence = new TendingPresence(spirit, old.Energy, old.CardPlays);
		spirit.RemoveMod<RiversDomain>();
	}

	// - are considered Sacred Sites
	public override bool IsSacredSite(Space space) => base.IsSacredSite(space)
		|| ((TendingPresenceToken)Token).IsTended(space);

	class TendingPresenceToken(Spirit spirit) : SpiritPresenceToken(spirit), IHandleTokenAdded, IHandleTokenRemoved {

		// Your lans with 4 or more Dahan....
		public bool IsTended(Space space) => 1 <= space[this]
			&& 4 <= space.Sum(TokenCategory.Dahan);

		Task IHandleTokenAdded.HandleTokenAddedAsync(Space to, ITokenAddedArgs args) {
			if(args.To is Space space)
				Update(space);
			return Task.CompletedTask;
		}

		public override Task HandleTokenRemovedAsync(ITokenRemovedArgs args) {
			if(args.From is Space space)
				Update(space);
			return base.HandleTokenRemovedAsync(args);
		}

		void Update(Space space) {
			int desiredDefend = IsTended(space) ? 1 : 0;
			space.Init(defendToken, desiredDefend);
		}

		readonly TokenVariety defendToken = new TokenVariety(SpiritIsland.Token.Defend, "💧");
	}

}
