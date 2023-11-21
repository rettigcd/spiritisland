namespace SpiritIsland.NatureIncarnate;

public class ExhaleConfusionAndDelirium {

	public const string Name = "Exhale Confusion and Delirium";

	[SpiritCard(Name, 0, Element.Sun,Element.Moon,Element.Air,Element.Animal),Fast,FromPresence(1,Target.Strife)]
	[Instructions("2 Fear. Invaders with Strife don't participate in Ravage."), Artist(Artists.EmilyHancock)]
	static public Task ActionAsync( TargetSpaceCtx ctx){
		ctx.AddFear(2);

		ctx.Tokens.Init(new BlightedDontRavage(),1);

		return Task.CompletedTask;
	}

	class BlightedDontRavage : ISkipRavages, IEndWhenTimePasses {
		public UsageCost Cost => UsageCost.Free;

		public Task<bool> Skip( SpaceState space ) {
			
			var blightedCounts = space.Humans()
				.Where(x=>0<x.StrifeCount)
				.ToDictionary( x => x, x => space[x] )
				.ToCountDict();

			SitOutRavage.SitOutNextRavage(space, blightedCounts);

			return Task.FromResult(false); // don't skip
		}
	}

}
