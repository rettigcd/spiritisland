using System;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class InfestationOfVenomousSpiders {

		[MajorCard("Infestation of Venomous Spiders",4,Element.Air,Element.Earth,Element.Plant,Element.Animal), Fast, FromSacredSite(2,Target.Invaders)]
		public static async Task ActAsync(TargetSpaceCtx ctx ) {
			// add 1 beast
			await ctx.Beasts.Add(1);

			// Gather up to 1 beast.
			await ctx.GatherUpTo(1, TokenType.Beast);

			// if you have 2 air 2 earth 3 animal: after this power causes invaders to skip an action, 4 damage.
			Func<GameState,Space,Task> causeAdditionalDamage = await ctx.YouHave("2 air,3 animal")
				? (GameState gs,Space space) => new SelfCtx(ctx.Self,gs,Cause.Power).Target(space).DamageInvaders(4)
				: null;

			// For each beast,
			int count = ctx.Beasts.Count;
			// 1 fear (max 4) and
			ctx.AddFear( System.Math.Min(4,count) );
			for(int i = 0; i < count; ++i) {
				// Invaders skip one Action in target land.
				string skipPhase = await ctx.Self.SelectText("Select Invader Phase to skip.", new[] { "Ravage","Build","Explore" },Present.Always);
				switch(skipPhase) {
					case "Ravage": ctx.SkipRavage( causeAdditionalDamage ); break;
					case "Build": ctx.Skip1Build( causeAdditionalDamage ); break;
					case "Explore": ctx.SkipExplore( causeAdditionalDamage ); break;
				}
			}

		}

	}


}
