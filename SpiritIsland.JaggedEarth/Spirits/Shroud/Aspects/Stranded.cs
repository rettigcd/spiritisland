
namespace SpiritIsland.JaggedEarth;

public class Stranded : IAspect {

	// https://spiritislandwiki.com/index.php?title=Stranded

	static public AspectConfigKey ConfigKey => new AspectConfigKey(ShroudOfSilentMist.Name, Name);
	public const string Name = "Stranded";
	public string[] Replaces => [MistsShiftAndFlow.Name];

	public void ModSpirit(Spirit spirit) {
		spirit.Targetter = new Targetter(spirit); // removes MistsShiftAndFlow
		spirit.Mods.Add( new MistsSteadilyDrift(spirit) );
		spirit.Mods.Add( new StrandedInTheShiftingMists(spirit) );

		spirit.SpecialRules = [
			..spirit.SpecialRules.Where(x=>x.Title != MistsShiftAndFlow.Name),
			MistsSteadilyDrift.Rule,
			StrandedInTheShiftingMists.Rule
		];
	}

}
