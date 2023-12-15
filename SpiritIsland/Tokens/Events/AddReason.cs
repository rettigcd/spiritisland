namespace SpiritIsland;

public enum AddReason {
	None,          // default / unspecified
	Added,         // Generic add
	Explore,       // invaders
	Build,         // invaders
	Ravage,        // blight from ravage
	BlightedIsland,// blight from blighted island card
	AddedToCard,   // destroyed presenece, possibly blight card too someday
	AsReplacement, // 
	SpecialRule,   // Heart of wildfire - Blight from add presence
	MovedTo,
}


