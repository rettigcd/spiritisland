using System.Security.Cryptography;

namespace SpiritIsland.JaggedEarth;

[InnatePower("Forsake Society to Chase After Dreams", "After this Power replaces pieces with explorer: Gather any number of those explorer into your lands.  If target land has any town/city remaining, 1 fear.")]
[Slow,FromPresence(1,Filter.Invaders)]
[RepeatIf("4 air")]
public class ForsakeSocietyToChaseAfterDreams {

	// after this power replaces pieces with explorer, Gather any number of those explorers into your lands.
	// If target land has any town/city remaining, 1 fear.

	[InnateTier("2 moon","Replace 1 explorer with 1 explorer.")]
	public static Task Option1(TargetSpaceCtx ctx ) {
		return Dissolve(ctx,Human.Explorer);
	}

	[InnateTier("2 moon,1 air","Instead, replace 1 town with 2 explorer.")]
	public static Task Option2(TargetSpaceCtx ctx ) {
		return Dissolve(ctx,Human.Explorer_Town);
	}

	// 4 moon 2 air 1 animal - instead, replace 1 city with 3 explorer.
	[InnateTier("4 moon,2 air,1 animal","Instead, replace 1 city with 3 explorer.")]
	public static Task Option3(TargetSpaceCtx ctx ) {
		return Dissolve(ctx,Human.Invader);
	}

	static async Task Dissolve(TargetSpaceCtx ctx, params HumanTokenClass[] invaderCats) {
		var decision = An.Invader.ToReplace("dissolve", ctx.Tokens.HumanOfAnyTag( invaderCats ).On(ctx.Space) );
		var invader = (await ctx.SelectAsync(decision))?.Token.AsHuman();
		if(invader == null) return;

		// Replace
		if(invader.HumanClass != Human.Explorer) {
			int numberOfExplorersToAdd = Math.Max(0,invader.HumanClass.ExpectedHealthHint - invader.Damage);
			await ctx.Invaders.Remove( invader, 1, RemoveReason.Replaced );
			await ctx.AddDefault( Human.Explorer, numberOfExplorersToAdd, AddReason.AsReplacement );
		}

		// Push to new land
		await ctx.SourceSelector
			.AddGroup(invader.RemainingHealth, Human.Explorer)
			.ConfigDestination(d=>d.FilterDestination( ctx.Self.Presence.IsOn ))
			.PushUpToN(ctx.Self);

		// If town/city remain, 1 fear.
		if( ctx.Tokens.HasAny(Human.Town_City) )
			ctx.AddFear(1);
	}

}