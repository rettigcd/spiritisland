using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Art;

public class ArtGen_Tests {

    [Fact(Skip="too slow")]
    public void Blight_Card(){
        using var Img = ResourceImages.Singleton.GetHealthBlightCard();
#pragma warning disable CS0642 // Possible mistaken empty statement
		using( ResourceImages.Singleton.GetBlightCard( new TheBorderOfLifeAndDeath() ) );
		using( ResourceImages.Singleton.GetBlightCard( new UntendedLandCrumbles() ) );
#pragma warning restore CS0642 // Possible mistaken empty statement
	}

}