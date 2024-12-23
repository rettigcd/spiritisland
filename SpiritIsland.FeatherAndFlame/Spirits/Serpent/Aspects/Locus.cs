
namespace SpiritIsland.FeatherAndFlame;

public class Locus : IAspect {

	// https://spiritislandwiki.com/index.php?title=Locus

	public const string Name = "Locus";
	static public AspectConfigKey Key => new AspectConfigKey(SerpentSlumbering.Name, Name);
	public string[] Replaces => [SerpentWakesInPower.Name];

	public void ModSpirit(Spirit spirit) {
		// Replace Elemental Aegis with Pull Beneath the Hungry EarthPull Beneath the Hungry Earth (Minor Power).
		spirit.ReplaceCard(ElementalAegis.Name,PowerCard.ForDecorated(PullBeneathTheHungryEarth));

		// Put your Incarna and the Presence from the Fire Element space of your Presence track on your starting board in land #5.
		spirit.ReplaceIncarna(new Incarna(spirit,"ss",Img.S_Incarna, Img.S_Incarna_Empowered)); // !!! wrong! - Incarna needs "Can be counted as badlands"
		spirit.AddActionFactory(new SpiritAction("Place Incarna and Fire Energy", PlaceIncarnaAndFireEnergy_Setup).ToGrowth());

		// Locus of the Serpent's Regard
		// Spirits with absorbed Presence can use your Presence at your Incarna for targeting.
		spirit.Mods.Add( new LocusOfTheSerpentsRegard() );

		// After uncovering the Earth Element space of your Presence track, Empower your Incarna.
		spirit.Presence.Energy.Slots.Skip(4).First().OnRevealAsync = (t,s) => { s.Incarna.Empowered = true; return Task.CompletedTask; };

		// Bonus Presence Track Icon   Add/Move your Incarna to a Land with your Presence
		spirit.Presence.Energy.Slots.First().Action = new MoveIncarnaToPresence(false);

		spirit.ReplaceInnate( SerpentWakesInPower.Name, InnatePower.For(typeof(StrengthOfTheWakingIsland)) );

		spirit.SpecialRules = [..spirit.SpecialRules, LocusOfTheSerpentsRegard.Rule];
	}


	static public Task PlaceIncarnaAndFireEnergy_Setup(Spirit spirit) {
		var space5 = GameState.Current.Island.Boards[0][5].ScopeSpace;
		space5.Init(spirit.Incarna, 1);
		return new Move( new TrackPresence( spirit.Presence.Energy.RevealOptions.First(), spirit.Presence.Token), space5 ).Apply();
	}



	// Duplicated here so we don't need to take dependency on Basegame
	[MinorCard("Pull Beneath the Hungry Earth", 1, Element.Moon, Element.Water, Element.Earth), Slow, FromPresence(1, Filter.Any)]
	[Instructions("If target land has your Presence, 1 Fear and 1 Damage. If target land is Sands / Wetland, 1 Damage."), Artist(Artists.NolanNasser)]
	static async Task PullBeneathTheHungryEarth(TargetSpaceCtx ctx) {

		int damage = 0;

		// If target land has your presence, 1 fear and 1 damage
		if( ctx.Presence.IsHere ) {
			++damage;
			await ctx.AddFear(1);
		}

		// If target land is Sand or Water, 1 damage
		if( ctx.IsOneOf(Terrain.Sands, Terrain.Wetland) )
			++damage;

		await ctx.DamageInvaders(damage);

	}

}