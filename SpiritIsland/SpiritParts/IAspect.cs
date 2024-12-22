namespace SpiritIsland;

/// <summary>
/// Used by Game Config to select Aspect and map to spirit.
/// </summary>
/// <param name="Spirit">Spirits Name</param>
/// <param name="Name">Aspect Name</param>
public record AspectConfigKey(string Spirit,string Aspect);

public interface IAspect {
	void ModSpirit( Spirit spirit );
	string[] Replaces { get; }
}

public static class AspectHelper_Extensoins {

	static public void RemoveMod<T>(this Spirit spirit) where T:ISpiritMod {
		var mod = spirit.Mods.OfType<T>().Single();
		spirit.Mods.Remove(mod);
	}

	static public void RemoveRule(this Spirit spirit, string ruleTitle) {
		spirit.SpecialRules = spirit.SpecialRules.Where(x => x.Title != ruleTitle).ToArray();
	}

	static public void ReplaceInnate(this Spirit spirit, string oldInnateName, InnatePower newInnate) {
		for(int i = 0; i < spirit.InnatePowers.Length; ++i ) {
			if( spirit.InnatePowers[i].Title == oldInnateName ) {
				spirit.InnatePowers[i] = newInnate;
			}
		}
	}

	static public void ReplaceIncarna(this Spirit spirit, Incarna newIncarna) {
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit,old.Energy,old.CardPlays,old.Token,newIncarna);
	}

	static public void ReplacePresenceToken(this Spirit spirit, SpiritPresenceToken token) {
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays, token, old.Incarna);
	}


	static public void ReplaceCard(this Spirit spirit, string oldCardName, PowerCard newCard) {
		for( int i = 0; i < spirit.Hand.Count; ++i ) {
			if( spirit.Hand[i].Title == oldCardName ) {
				spirit.Hand[i] = newCard;
			}
		}
	}

	static public void ReplaceRule(this Spirit spirit, string oldRuleTitle, SpecialRule newRule) {
		for( int i = 0; i < spirit.SpecialRules.Length; ++i ) {
			if( spirit.SpecialRules[i].Title == oldRuleTitle ) {
				spirit.SpecialRules[i] = newRule;
			}
		}
	}

	static public void AddInnate(this Spirit spirit, InnatePower newInnate) {
		spirit.InnatePowers = [.. spirit.InnatePowers, newInnate];
	}

	static public void RemoveCustomPresence(this Spirit spirit) {
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays, old.Token);
	}

	static public void RemovePresenceToken(this Spirit spirit) {
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays);
	}

}