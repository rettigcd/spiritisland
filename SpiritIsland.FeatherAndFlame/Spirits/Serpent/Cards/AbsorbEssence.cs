namespace SpiritIsland.FeatherAndFlame;
	
public class AbsorbEssence {

	[SpiritCard("Absorb Essence",2,Element.Moon, Element.Fire, Element.Water, Element.Earth),Fast,AnotherSpirit]
	[Instructions( "Gain 3 Energy. Move 1 of target Spirit's Presence from the board to your \"Deep Slumber\" track. Absorbed Presence cannot be returned to play. Target Spirit gains 1 Any and 1 Energy." ), Artist( Artists.JorgeRamos )]
	static public async Task ActAsync(TargetSpiritCtx ctx) {
		// gain 3 energy.
		ctx.Self.Energy += 3;

		// if card is traded to someone else, don't crash
		if(ctx.Self.Presence is not SerpentPresence serpentPresence) return;

		if(6 <= serpentPresence.AbsorbedPresences.Count) return;

		// move 1 of target spirit's presence from the board to your 'Deep Slumber' track.
		// Absorbed presence cannot be returned to play.
		var space = await ctx.Other.SelectDeployed("Select presence to be absorbed");
		await ctx.Other.Presence.Token.RemoveFrom(space.ScopeTokens); // !!! maybe should allow Incarna here too.
		serpentPresence.AbsorbedPresences.Add(ctx.Other);

		// Target spirit gains 1 ANY and 1 energy
		ctx.Other.Energy += 1;
		ctx.Other.Elements.Add(Element.Any);  // This is converted when it is needed.
	}

}