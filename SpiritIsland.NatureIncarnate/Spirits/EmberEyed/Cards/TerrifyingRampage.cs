namespace SpiritIsland.NatureIncarnate;

public class TerrifyingRampage {

	public const string Name = "Terrifying Rampage";

	[SpiritCard(Name, 1, Element.Moon,Element.Fire,Element.Earth),Fast,FromPresence(1)]
	[Instructions("1 Fear. 2 Invaders don't participate in Ravage - choose when Ravaging. Push 3 Dahan."), Artist(Artists.DavidMarkiwsky)]
	static public async Task ActionAsync( TargetSpaceCtx ctx){
		// 1 Fear.
		await ctx.AddFear(1);
		// 2 Invaders don't participate in Ravage. (Choose when ravaging.)
		ctx.Space.Adjust(new InvadersSitOut(
			ctx.Self,
			new Quota().AddGroup(2,Human.Invader)
		),1);
		await ctx.PushDahan(3);
	}

	class InvadersSitOut( Spirit invaderPicker, Quota quota ) : BaseModEntity, IConfigRavages, IEndWhenTimePasses {

		readonly Spirit _invaderPicker = invaderPicker;
		readonly Quota _quota = quota;

		async Task IConfigRavages.Config( Space space ) {
			await space.SourceSelector
				.UseQuota( _quota )
				.SelectFightersAndSitThemOut( _invaderPicker );
		}

	}

}
