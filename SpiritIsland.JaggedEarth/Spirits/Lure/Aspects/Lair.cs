
namespace SpiritIsland.JaggedEarth;

public class Lair : IAspect {

	public const string Name = "Lair";
	static public AspectConfigKey ConfigKey => new AspectConfigKey(LureOfTheDeepWilderness.Name,Name);
	public string[] Replaces => [];

	public void ModSpirit(Spirit spirit) {
		// Replace 3 presence with 1 Lair
		spirit.AddActionFactory(new SpiritAction("Init Lair", Setup).ToGrowth());

		// Create Incarna
		var incarna = new ASingleAluringLair(spirit);
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays, new SpiritPresenceToken(spirit), incarna);
		spirit.Mods.Add(incarna);

		// replace innate
		spirit.ReplaceInnate(ForsakeSocietyToChaseAfterDreams.Name, InnatePower.For(typeof(SocietyDissolvesAtTheBeckoningHeart)));

		spirit.ReplaceRule(EnthrallTheForeignExplorers.Name, ASingleAluringLair.Rule);
	}

	static async Task Setup( Spirit spirit ) {
		var board = GameState.Current.Island.Boards[0]; // !!! assumes single spirit.
		var options = board.Spaces.Where(s=>!s.IsCoastal).ScopeTokens();
		var lairSpace = await spirit.SelectAlways("Place Lair", options);
		lairSpace.Init(spirit.Incarna,1);

		foreach(var land in spirit.Presence.Lands.ToArray())
			land.Init(spirit.Presence.Token,0);
	}

}
