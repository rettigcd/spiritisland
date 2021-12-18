using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("Forsake Society to Chase After Dreams"),Slow,FromPresence(1,Target.Invaders)]
	[RepeatIf("4 air")]
	public class ForsakeSocietyToChaseAfterDreams {

		// after this power replaces pieces with explorer, Gather any number of those explorers into your lands.
		// If target land has any town/city remaining, 1 fear.

		[InnateOption("2 moon","Replace 1 explorer with 1 explorer.")]
		public static Task Option1(TargetSpaceCtx ctx ) {
			return Dissolve(ctx,Invader.Explorer);
		}

		[InnateOption("2 moon,1 air","Instead, replace 1 town with 2 explorer.")]
		public static Task Option2(TargetSpaceCtx ctx ) {
			return Dissolve(ctx,Invader.Town,Invader.Explorer);
		}

		// 4 moon 2 air 1 animal - instead, replace 1 city with 3 explorer.
		[InnateOption("4 moon,2 air,1 animal","Instead, replace 1 city with 3 explorer.")]
		public static Task Option3(TargetSpaceCtx ctx ) {
			return Dissolve(ctx,Invader.City,Invader.Town,Invader.Explorer);
		}

		static async Task Dissolve(TargetSpaceCtx ctx, params TokenCategory[] groups) {
			var decision = Select.Invader.ToDowngrade("dissolve", ctx.Space, ctx.Tokens.OfAnyType( groups ) );
			var token = await ctx.Decision( decision );
			if(token == null) return;

			if(token != Invader.Explorer.Default) {
				await ctx.Tokens.Remove(token,1,RemoveReason.Replaced);
				await ctx.Tokens.Add(Invader.Explorer.Default,token.Health, AddReason.AsReplacement);
			}

			await ctx.Pusher
				.AddGroup( token.Health, Invader.Explorer )
				.FilterDestinations( ctx.Self.Presence.IsOn )
				.MoveUpToN();
		}

	}


}
