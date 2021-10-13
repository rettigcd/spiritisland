using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	[InnatePower( SerpentWakesInPower.Name ), Slow, Yourself]
	public class SerpentWakesInPower {

		public const string Name = "Serpent Wakes in Power";

		[InnateOption( "2 fire,1 water,1 plant","Gain 1 Energy. Other spirits with any Absorbed Presence also gain 1 Energy." )]
		static public Task Option1Async( TargetSpaceCtx ctx ) {
			// Gain 1 Energy.
			ctx.Self.Energy += 1;

			// Other spirits with any Absorbed Presence also gain 1 Energy.
			var presence = (SerpentPresence)ctx.Self.Presence;
			foreach(var spirit in presence.AbsorbedPresences.Distinct())
				spirit.Energy += 1;

			return Task.CompletedTask;
		}

		[InnateOption( "2 water,3 earth,2 plant", "Add 1 presence to range-1.  Other spirits with 2 or more Absobred Presence may do likewise." )]
		static public async Task Option2Async( TargetSpaceCtx ctx ) {
			await Option1Async( ctx );

			// Add 1 presence, range-1.
			await ctx.PlacePresence(1,Target.Any);

			// Other spirits with 2 or more Absobred Presence may do likewise.
			var presence = (SerpentPresence)ctx.Self.Presence;
			var qualifyingSpirits = presence.AbsorbedPresences.GroupBy(x=>x).Where(grp=>2<=grp.Count()).Select(grp=>grp.Key);
			foreach(var spirit in presence.AbsorbedPresences.Distinct())
				await new SpiritGameStateCtx(spirit,ctx.GameState,Cause.Power).PlacePresence(1,Target.Any);

		}

		[InnateOption("3 fire,3 water,3 earth,3 plant", "Gain a Major Power without Forgetting.  Other Spirits with 3 or more Absorbed Presence may do likewise." )]
		static public async Task Option3Async( TargetSpaceCtx ctx ) {
			await Option2Async( ctx );

			// Gain a Major Power without Forgetting.
			await ctx.DrawMajor(4,false);

			// Other Spirits with 3 or more Absorbed Presence may do likewise."
			var presence = (SerpentPresence)ctx.Self.Presence;
			var qualifyingSpirits = presence.AbsorbedPresences.GroupBy(x=>x).Where(grp=>3<=grp.Count()).Select(grp=>grp.Key);
			foreach(var spirit in presence.AbsorbedPresences.Distinct())
				await new SpiritGameStateCtx(spirit,ctx.GameState,Cause.Power).DrawMajor(4,false);
		}

	}

}
