
namespace SpiritIsland.JaggedEarth;

public class Lair : IAspect {

	public const string Name = "Lair";
	static public AspectConfigKey ConfigKey => new AspectConfigKey(LureOfTheDeepWilderness.Name,Name);
	public string[] Replaces => [];

	static InnatePower NewInnate => InnatePower.For(typeof(SocietyDissolvesAtTheBeckoningHeart));
	public InnatePower[] NewInnates => [NewInnate];

	public void ModSpirit(Spirit spirit) {
		// Replace 3 presence with 1 Lair
		spirit.AddActionFactory(new InitLair().ToGrowth());

		// Create Incarna
		var incarna = new ASingleAluringLair(spirit);
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays, new SpiritPresenceToken(spirit), incarna);
		spirit.Mods.Add(incarna);

		// replace innate
		spirit.ReplaceInnate(ForsakeSocietyToChaseAfterDreams.Name, NewInnate);

		spirit.ReplaceRule(EnthrallTheForeignExplorers.Name, ASingleAluringLair.Rule);
	}

	// Named (rather than an anonymous `new SpiritAction(...)`) so it can implement ISerializableSelfCmd -
	// same reasoning/pattern as PlayCardForCost. A raw SpiritAction wraps an arbitrary delegate with
	// nothing to key a JSON round-trip off of; this one has no per-instance state, so its Tag alone
	// resolves it.
	internal class InitLair : SpiritAction, ISerializableSelfCmd {

		public InitLair() : base("Init Lair") { }

		public override async Task ActAsync(Spirit spirit) {
			var board = GameState.Current.Island.Boards[0]; // !!! assumes single spirit.
			var options = board.Spaces.Where(s=>!s.IsCoastal).ScopeTokens();
			var lairSpace = await spirit.SelectAlways("Place Lair", options);
			lairSpace.Init(spirit.Incarna,1);

			foreach(var land in spirit.Presence.Lands.ToArray())
				land.Init(spirit.Presence.Token,0);
		}

		const string Tag = "Lair.InitLair";

		JsonArray ISerializableSelfCmd.ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SelfCmdRegistry.Register( Tag, ( json, ctx ) => new InitLair() );

	}

}
