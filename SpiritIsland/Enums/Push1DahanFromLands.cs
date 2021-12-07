using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {
	/// <summary> Pushes 1 dahan from 1 of your lands. </summary>
	public class Push1DahanFromLands : GrowthActionFactory, ITrackActionFactory {

		public bool RunAfterGrowthResult => true; // depends on Presence location which might change during growth

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var dahanOptions = ctx.Self.Presence.Spaces
				.SelectMany(space=>ctx.Target(space).Dahan.Keys.Select(t=>new SpaceToken(space,t)));
			var source = await ctx.Self.Action.Decision(new Decision.SpaceTokens("Select dahan to push from land",dahanOptions,Present.Done));
			if(source == null) return;

			await new TokenPusher( ctx.Target(source.Space) ).PushToken( source.Token );
		}
	}

}
