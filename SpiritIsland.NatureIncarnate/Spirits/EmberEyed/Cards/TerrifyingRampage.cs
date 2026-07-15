namespace SpiritIsland.NatureIncarnate;

public class TerrifyingRampage {

	public const string Name = "Terrifying Rampage";

	[SpiritCard(Name, 1, Element.Moon,Element.Fire,Element.Earth),Fast,FromPresence(1)]
	[Instructions("1 Fear. 2 Invaders don't participate in Ravage - choose when Ravaging. Push 3 Dahan."), Artist(Artists.DavidMarkiwsky)]
	static public async Task ActAsync( TargetSpaceCtx ctx){
		// 1 Fear.
		await ctx.AddFear(1);
		// 2 Invaders don't participate in Ravage. (Choose when ravaging.)
		ctx.Space.Adjust(new InvadersSitOut(ctx.Self),1);
		await ctx.PushDahan(3);
	}

	public class InvadersSitOut( Spirit invaderPicker ) : BaseModEntity, IConfigRavages, IEndWhenTimePasses {

		readonly Spirit _invaderPicker = invaderPicker;

		async Task IConfigRavages.Config( Space space ) {
			await space.SourceSelector
				.UseQuota( new Quota().AddGroup(2,Human.Invader) )
				.SelectFightersAndSitThemOut( _invaderPicker );
		}

	}

}
