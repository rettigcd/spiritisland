namespace SpiritIsland.JaggedEarth;

public class ScreamDiseaseIntoTheWind{

	public const string Name = "Scream Disease Into the Wind";

	[MinorCard(Name,1,Element.Air,Element.Water,Element.Animal),Fast,AnotherSpirit]
	[Instructions( "Target Spirit gets +1 Range with all their Powers. Once this turn, after target Spirit uses a Power targeting a land, they may add 1 Disease to that land. (Hand them a Disease token as a reminder.)" ), Artist( Artists.MoroRogers )]
	static public Task ActAsync(TargetSpiritCtx ctx){
		// Target Spirit gets +1 range with all their Powers.
		RangeCalcRestorer.Save(ctx.Other);
		RangeExtender.Extend( ctx.Other, 1 );

		RunSpaceActionOnceOnFutureTarget.Trigger(ctx.Other,AddDisease);

		// (Hand them a disease token as a reminder.)
		return Task.CompletedTask;
	}

	static SpaceAction AddDisease => new SpaceAction(ScreamDiseaseIntoTheWind.Name,AddDiseaseImp);

	static Task AddDiseaseImp(TargetSpaceCtx ctx) => ctx.Space.Disease.AddAsync(1);

}
