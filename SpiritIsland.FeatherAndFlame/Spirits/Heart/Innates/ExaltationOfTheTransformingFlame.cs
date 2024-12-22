namespace SpiritIsland.FeatherAndFlame;

[InnatePower(Name), Fast,AnotherSpirit]
public class ExaltationOfTheTransformingFlame {

	public const string Name = "Exaltation of the Transforming Flame";

	[InnateTier("4 fire,1 plant", "Target Spirit may Forget a Power Card to gain a Power Card and 1 of Any Element. You may do likewise.")]
	static public async Task Option1(TargetSpiritCtx ctx) {
		// Target Spirit may Forget a Power Card to gain a Power Card and 1 of Any Element.
		await ForgetAPowerCardToGainAPowerCardPlusAnyElement(ctx.Other);
		// You may do likewise.
		await ForgetAPowerCardToGainAPowerCardPlusAnyElement(ctx.Self);
	}

	static async Task ForgetAPowerCardToGainAPowerCardPlusAnyElement(Spirit spirit) {
		if( await spirit.UserSelectsFirstText("Gain Power Card + ANY Element by Forgetting a Power Card", "Yes, Forget Power Card", "No Thanks.")
			&& await spirit.Forget.ACard() is not null
		) {
			await spirit.Draw.Card();
			spirit.Elements.Add(Element.Any);
		}
	}

	[InnateTier("3 fire,1 earth,2 plant", "Target Spirit may pay 1 Energy to Replace 1 Blight with 1 Badlands in one of their lands. You may do likewise.", 1)]
	static public async Task Option2(TargetSpiritCtx ctx) {
		// Target Spirit may pay 1 Energy to Replace 1 Blight with 1 Badlands in one of their lands.
		await Pay1EnergyToReplace1BlightInLands(ctx.Other);
		// You may do likewise.
		await Pay1EnergyToReplace1BlightInLands(ctx.Self);
	}

	static async Task Pay1EnergyToReplace1BlightInLands(Spirit spirit) {
		if( spirit.Energy < 1 ) return;
		var blight = await spirit.Select("Pay 1 Energy to replace Blight with Badlands", spirit.Presence.Lands.SelectMany(x => x.SpaceTokensOfTag(Token.Blight)), Present.Done);
		if( blight is null ) return;

		--spirit.Energy;
		await blight.Space.ReplaceAsync(blight.Token, 1, Token.Badlands);
	}


	[InnateTier("1 sun,3 fire,1 animal", "Target Spirit may Replace 1 Explorer with 1 Beasts in one of their lands. You may do likewise.")]
	static public async Task Option3(TargetSpiritCtx ctx) {
		// Target Spirit may Replace 1 Explorer with 1 Beasts in one of their lands.
		await ReplaceExplorerWithBeast(ctx.Other);
		// You may do likewise.
		await ReplaceExplorerWithBeast(ctx.Self);
	}

	static async Task ReplaceExplorerWithBeast(Spirit spirit) {
		var explorer = await spirit.Select("Replace Explorer with Beast", spirit.Presence.Lands.SelectMany(x => x.SpaceTokensOfTag(Human.Explorer)), Present.Done);
		if( explorer is null ) return;

		await explorer.Space.ReplaceAsync(explorer.Token, 1, Token.Beast);
	}

}
