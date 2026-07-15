

namespace SpiritIsland.Basegame;

public class FrightfulShadowsEludeDestruction(Spirit spirit) : SpiritPresenceToken(spirit), IModifyRemovingToken, ISpiritMod, ICleanupSpiritWhenTimePasses {

	public const string Name = "Frightful Shadows Elude Destruction";
	const string Description = "The first time each Action would destroy your Presence, you may Push 1 of those Presence instead of destroying it.";
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	static public void InitAspect(Spirit spirit) {
		var old = spirit.Presence;
		var token = new FrightfulShadowsEludeDestruction(spirit);
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays, token);
		// Not otherwise in spirit.Mods (it's the Presence token, dispatched via Space token events instead) 
		// - added here solely so ICleanupSpiritWhenTimePasses resets UsedThisRound each round.
		spirit.Mods.Add(token);
	}

	async Task IModifyRemovingToken.ModifyRemovingAsync(RemovingTokenArgs args) {
		if(args.Token == this
			&& args.Reason.IsDestroyingPresence()
			&& !UsedThisRound
		) {

			var dst = await Self.Select("Instead of destroying, push presence to:", args.From.Adjacent, Present.Done);
			if( dst is null ) return;

			--args.Count;
			await args.Token.On(args.From).MoveTo(dst);
			UsedThisRound = true;
		}
	}

	bool UsedThisRound;

	void ICleanupSpiritWhenTimePasses.CleanupSpirit( Spirit spirit ) => UsedThisRound = false;

}