
namespace SpiritIsland.JaggedEarth;

public class ASingleAluringLair(Spirit spirit) : Incarna(spirit, "L", Img.L_Incarna, Img.L_Incarna)
	, IConfigRavages
	, IModifyRemovingToken
	, ICreateSacredSites
{

	public const string Name = "A Single Alluring Lair";
	const string Description = "Invaders/Dahan/Beasts/Presence at your Incarna can't move. (You can reposition your Incarna by adding it with your second Growth option instead of adding Presence.)"
		+" 6 Explorers and 3 Dahan at your Incarna don't participate in Ravage. (They don't deal Damage or take Damage.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	public Task ModifyRemovingAsync(RemovingTokenArgs args) {
		// Invaders/Dahan/Beasts/Presence at your Incarna can't move. 
		if( args.Reason == RemoveReason.MovedFrom	// Moving
			&& args.Token != this					// the Lair Incarna (because generic 'presence' is stuck)
			&& IsStuckToken(args.Token)				// Stuck
		) {
			args.Count = 0;
		}
		return Task.CompletedTask;
	}

	static bool IsStuckToken(IToken token) {
		return token is HumanToken || token.HasTag(Token.Beast) || token.HasTag(TokenCategory.Presence);
	}

	public Task Config(Space space) {
		// 6 Explorers and 3 Dahan at your Incarna don't participate in Ravage. (They don't deal Damage or take Damage.
		return space.SourceSelector
			.AddGroup(6, Human.Explorer)
			.AddGroup(3, Human.Dahan)
			.SelectFightersAndSitThemOut(Self,Present.Always);
	}

	bool ICreateSacredSites.IsSacredSite(Space space) => Space == space;
}