namespace SpiritIsland.JaggedEarth;

[InnatePower("Forsake Society to Chase After Dreams", "After this Power replaces pieces with explorer: Gather any number of those explorer into your lands.  If target land has any town/city remaining, 1 fear.")]
[Slow,FromPresence(1,Target.Invaders)]
[RepeatIf("4 air")]
public class ForsakeSocietyToChaseAfterDreams {

	// after this power replaces pieces with explorer, Gather any number of those explorers into your lands.
	// If target land has any town/city remaining, 1 fear.

	[InnateOption("2 moon","Replace 1 explorer with 1 explorer.")]
	public static Task Option1(TargetSpaceCtx ctx ) {
		return Dissolve(ctx,Human.Explorer);
	}

	[InnateOption("2 moon,1 air","Instead, replace 1 town with 2 explorer.")]
	public static Task Option2(TargetSpaceCtx ctx ) {
		return Dissolve(ctx,Human.Explorer_Town);
	}

	// 4 moon 2 air 1 animal - instead, replace 1 city with 3 explorer.
	[InnateOption("4 moon,2 air,1 animal","Instead, replace 1 city with 3 explorer.")]
	public static Task Option3(TargetSpaceCtx ctx ) {
		return Dissolve(ctx,Human.Invader);
	}

	static async Task Dissolve(TargetSpaceCtx ctx, params HumanTokenClass[] invaderCats) {
		var decision = Select.Invader.ToReplace("dissolve", ctx.Space, ctx.Tokens.OfAnyHumanClass( invaderCats ) );
		var invader = (HumanToken)(await ctx.Decision( decision ))?.Token;
		if(invader == null) return;

		// Replace
		if(invader.Class != Human.Explorer) {
			await ctx.Invaders.Remove(invader,1,RemoveReason.Replaced);
			await ctx.AddDefault( Human.Explorer,invader.RemainingHealth, AddReason.AsReplacement );
		}

		// !!! If they are damaged, should we distribute that damage and destroy some of the explorers?

		// Push to new land
		await ctx.Pusher
			.AddGroup( invader.RemainingHealth, Human.Explorer )
			.FilterDestinations( ctx.Self.Presence.IsOn )
			.MoveUpToN();

		// If town/city remain, 1 fear.
		if( ctx.Tokens.HasAny(Human.Town_City) )
			ctx.AddFear(1);
	}

}