namespace SpiritIsland.Tests;

static public class PresenceTrackExtensoins {

	static public void SetRevealedCount( this IPresenceTrack sut, int value ) {
		while(sut.Revealed.Count()<value)
			sut.Reveal(sut.RevealOptions.Single(), null);
	}

}
