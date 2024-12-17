namespace SpiritIsland.NatureIncarnate;

public class ExhaleConfusionAndDelirium {

	public const string Name = "Exhale Confusion and Delirium";

	[SpiritCard(Name, 0, Element.Sun,Element.Moon,Element.Air,Element.Animal),Fast,FromPresence(1,Filter.Strife)]
	[Instructions("2 Fear. Invaders with Strife don't participate in Ravage."), Artist(Artists.EmilyHancock)]
	static public async Task ActionAsync( TargetSpaceCtx ctx){
		await ctx.AddFear(2);
		ctx.Space.Init(new BlightedInvadersSitOutRavage(),1);
	}

	class BlightedInvadersSitOutRavage : IConfigRavages, IEndWhenTimePasses {

		Task IConfigRavages.Config( Space space ) {
			
			var blightedCounts = space.AllHumanTokens()
				.Where(x=>0<x.StrifeCount)
				.ToDictionary( x => x, x => space[x] )
				.ToCountDict();

			SitOutRavage.SitOutThisRavageAction(space, blightedCounts);
			return Task.CompletedTask;
		}
	}

}
