using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {
	public class HeartPresence : SpiritPresence {

		public HeartOfTheWildfire spirit;

		public HeartPresence( PresenceTrack energy, PresenceTrack cardPlays )
			:base( energy, cardPlays )
		{ }

		public int FireShowing() {
			var dict = new CountDictionary<Element>();
			Energy.AddElements(dict);
			CardPlays.AddElements(dict);
			return dict[Element.Fire];
		}

		public override async Task PlaceFromBoard( IOption from, Space to, GameState gs ) {
			await base.PlaceFromBoard( from, to, gs );

			int fireCount = FireShowing();
			var ctx = new SpiritGameStateCtx(spirit,gs,Cause.Growth).Target(to);
			// For each fire showing, do 1 damage
			await ctx.DamageInvaders(fireCount);
			// if 2 fire or more are showing, add 1 blight
			if(2<=fireCount)
				await ctx.AddBlight(1);
		}

		// !!! Blight added due to Spirit Effects (Powers, Special Rules, Scenario-based Rituals, etc) does not destroy your presence. (including cascades)
		// When Destroying presence from blight, need Cause so we can tell if destoryoing it due to Ravage or something else.
	}

}
