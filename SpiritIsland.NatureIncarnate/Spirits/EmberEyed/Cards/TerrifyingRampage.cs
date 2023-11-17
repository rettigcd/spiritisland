namespace SpiritIsland.NatureIncarnate;

public class TerrifyingRampage {

	public const string Name = "Terrifying Rampage";

	[SpiritCard(Name, 1, Element.Moon,Element.Fire,Element.Earth),Fast,FromPresence(1)]
	[Instructions("1 Fear. 2 Invaders don't participate in Ravage - choose when Ravaging. Push 3 Dahan."), Artist(Artists.DavidMarkiwsky)]
	static public async Task ActionAsync( TargetSpaceCtx ctx){
		// 1 Fear.
		ctx.AddFear(1);
		// 2 Invaders don't participate in Ravage. (Choose when ravaging.)
		ctx.Tokens.Adjust(new InvadersSitOut(
			ctx.Self,
			new Quota().AddGroup(2,Human.Invader)
		),1);
		await ctx.PushDahan(3);
	}

	class InvadersSitOut : BaseModEntity, ISkipRavages, IEndWhenTimePasses {

		readonly Spirit _invaderPicker;
		readonly Quota _quota;

		public InvadersSitOut(Spirit invaderPicker,Quota quota) {
			_invaderPicker = invaderPicker;
			_quota = quota;
		}

		public UsageCost Cost => UsageCost.Free;

		public async Task<bool> Skip( SpaceState space ) {

			var sourceSelector = new SourceSelector( space )
				.NotRemoving()
				.UseQuota( _quota );

			await SitOutRavage.SelectFightersAndSitThemOut( _invaderPicker, sourceSelector );

			return false; // does not stop ravage ever
		}

	}

}
