namespace SpiritIsland.Basegame;

public class Nourishing : IAspect {

	// https://spiritislandwiki.com/index.php?title=Nourishing

	static public AspectConfigKey ConfigKey => new AspectConfigKey(VitalStrength.Name, Name);
	public const string Name = "Nourishing";
	public string[] Replaces => [EarthsVitality.Name];

	public void ModSpirit(Spirit spirit) {
		EarthsVitality.ReplaceWith(spirit,new SpiritPresenceToken(spirit));

		// Replace A Year of Perfect Stillness with Voracious Growth(Minor Power)
		for(int i=0; i<spirit.Hand.Count;++i)
			if(spirit.Hand[i].Title == AYearOfPerfectStillness.Name)
				spirit.Hand[i] = PowerCard.ForDecorated(VoraciousGrowth.ActAsync);

		ImbueWithNourishingVitality.InitAspect(spirit);
		FlourishWithNaturesStrength.InitAspect(spirit);

		spirit.SpecialRules = [FlourishWithNaturesStrength.Rule];
	}

}
