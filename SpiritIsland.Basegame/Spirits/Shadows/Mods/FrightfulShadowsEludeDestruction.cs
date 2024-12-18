

namespace SpiritIsland.Basegame;

public class FrightfulShadowsEludeDestruction(Spirit spirit) : SpiritPresenceToken(spirit), IModifyRemovingToken {

	public const string Name = "Frightful Shadows Elude Destruction";
	const string Description = "The first time each Action would destroy your Presence, you may Push 1 of those Presence instead of destroying it.";
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	static public void InitAspect(Spirit spirit) {
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays, new FrightfulShadowsEludeDestruction(spirit));
	}

	async Task IModifyRemovingToken.ModifyRemovingAsync(RemovingTokenArgs args) {
		if(args.Token == this 
			&& args.Reason.IsDestroyingPresence() 
			&& !UsedThisRound
		) {

			var dst = await Self.SelectAsync(new A.SpaceDecision("Instead of destroying, push presence to:", args.From.Adjacent, Present.Done));
			if( dst is null ) return;

			--args.Count;
			await args.Token.MoveAsync(args.From, dst);
			UsedThisRound = true;
		}
	}

	static bool UsedThisRound {
		get => GameState.Current.RoundScope.ContainsKey(Name);
		set => GameState.Current.RoundScope[Name] = true;
	}

}