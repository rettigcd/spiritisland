namespace SpiritIsland;

public enum ActionCategory {

	Default, // nothing

	// Spirit
	Spirit_Growth,
	Spirit_Power,
	Spirit_SpecialRule, // which specified After X, do Y
	Spirit_PresenceTrackIcon,
	//	GainEnery, // specifiec on preence track
	//	SpiritualRituals,

	Invader,
	//	One Ravage, Build, or Explore in one land

	Blight,
	//	The effects of a Blight Card

	Fear,
	//	Everything one Fear Card does

	Event,
	//	Everything a Main Event Does
	//	Everything a Token Event does
	//	Everything a dahan event does

	Adversary,
	//	An adversary's escalation effects (except englind as it invokes a bild)
	//	Instructions on an adversary to perform some effect.
	//	Actions written on the scenario panel.

	Special
    // Command the Beasts

}
