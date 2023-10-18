using System.ComponentModel.Design;

namespace SpiritIsland;

public class RavageExchange {

	public RavageExchange( SpaceState tokens, RavageOrder order ) {
		Tokens = tokens;
		Order = order;
		StartingAttackers = GetSideParticipants( RavageSide.Attacker );
		ActiveAttackers = FindActive(StartingAttackers);
		StartingDefenders = GetSideParticipants( RavageSide.Defender ); 
		ActiveDefenders = FindActive( StartingDefenders );
	}

	public Space Space => Tokens.Space;
	readonly public SpaceState Tokens;
	readonly public RavageOrder Order;

	/// <summary> All participating attackers, not just the ones acitve this round. </summary>
	readonly public CountDictionary<HumanToken> StartingAttackers;
	/// <summary> All participating defenders, not just the ones acitve this round. </summary>
	readonly public CountDictionary<HumanToken> StartingDefenders;
	/// <summary> Just the participating attackers that are active this round. </summary>
	readonly public CountDictionary<HumanToken> ActiveAttackers;
	/// <summary> Just the participating defenders that are active this round. </summary>
	readonly public CountDictionary<HumanToken> ActiveDefenders;

	public CountDictionary<HumanToken> EndingDefenders;
	public CountDictionary<HumanToken> EndingAttackers;
	
	public bool HasActiveParticipants => 0 < (ActiveAttackers.Count + ActiveDefenders.Count);

	public int defend;

	public int damageFromAttackers; // post-defend.  Defend points already applied
	public int damageFromDefenders;

	public int defenderDamageFromBadlands;  // post-defend.  Defend points already applied
	public int attackerDamageFromBadlands;

	public int dahanDestroyed;

	CountDictionary<HumanToken> GetSideParticipants( RavageSide side ) {
		return Tokens.OfTypeHuman()
			.Where( token => token.RavageSide == side )
			.ToDictionary( x => x, x => Tokens[x] )
			.ToCountDict();
	}

	CountDictionary<HumanToken> FindActive( CountDictionary<HumanToken> all ) {
		return all.Keys
			.Where( token => token.RavageOrder == Order )
			.ToDictionary( x => x, x => Tokens[x] )
			.ToCountDict();
	}

	public override string ToString() {

		string verb = Order switch {
			RavageOrder.Ambush => "ambush",
			RavageOrder.InvaderTurn => "attack",
			RavageOrder.DahanTurn => "defend",
			_ => "Unknown"
		};

		var parts = new List<string>();
		parts.Add($"{verb}:");

		if(0 < ActiveAttackers.Count) {
			if(defend > 0)
				parts.Add( $"Defend {defend}." );

			// Attacker Effect
			string badlandString = 0 < defenderDamageFromBadlands ? $" plus {defenderDamageFromBadlands} badland damage" : string.Empty;
			parts.Add( $"({ActiveAttackers.TokenSummary()}) deal {damageFromAttackers} damage{badlandString} to defenders ({StartingDefenders.TokenSummary()}) destroying {dahanDestroyed} of them." );
		}

		// Defend Effect
		if(0 < ActiveDefenders.Count) { //	if(0 < damageFromDefenders) {
			string bld = (0 < attackerDamageFromBadlands) ? $" plus {attackerDamageFromBadlands} badland damage" : string.Empty;
			parts.Add( $"({ActiveDefenders.TokenSummary()}) deal {damageFromDefenders} damage{bld}, leaving {EndingAttackers.TokenSummary()}." );
		}

		return string.Join(" ",parts);
	}

}