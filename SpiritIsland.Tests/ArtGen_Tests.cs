using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests;

public class ArtGen_Tests {

    [Fact(Skip="too slow")]
    public void Blight_Card(){
        using var Img = ResourceImages.Singleton.GetHealthBlightCard();
        using( ResourceImages.Singleton.GetBlightCard( new TheBorderOfLifeAndDeath() ) );
        using( ResourceImages.Singleton.GetBlightCard( new UntendedLandCrumbles() ) );
    }

}