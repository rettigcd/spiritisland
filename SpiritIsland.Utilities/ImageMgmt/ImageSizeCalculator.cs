using System.Drawing;

namespace SpiritIsland;

/// <summary>
/// Loads images and caches their size, but does not hang on to the image.
/// </summary>
public class ImageSizeCalculator {

	public int IconDimension { get; set; }
	public int ElementDimension { get; set; }

	public ImageSizeCalculator(int iconDimension, int elementDimension ) {
		IconDimension = iconDimension;
		ElementDimension = elementDimension;
	}

	public (Size,Img) GetTokenDetails( string tokenName ) {
		Img img = SimpleWordToIcon( tokenName );
		var size = "sun|moon|air|fire|water|plant|animal|earth".Contains( tokenName )
			? new Size( ElementDimension, ElementDimension )          // elements get special size
			: CalcIconSize( img, IconDimension ); // non-elements must fit inside iconDimension
		return (size,img);
	}


	Size CalcIconSize( Img img, int maxHeight ) {
		if(!iconSizes.ContainsKey( img )) {
			using Image image = ResourceImages.Singleton.GetImg( img );
			iconSizes.Add( img, image.Size );
		}
		var sz = iconSizes[img];

		return true // sz.Width < sz.Height
			? new Size( maxHeight * sz.Width / sz.Height, maxHeight ) 
			: new Size( maxHeight, maxHeight * sz.Height / sz.Width );
	}

	static Img SimpleWordToIcon( string token ) {
		return token switch {
			"dahan" => Img.Icon_Dahan,
			"sacred site" => Img.Icon_Sacredsite,
			"city" => Img.Icon_City,
			"town" => Img.Icon_Town,
			"explorer" => Img.Icon_Explorer,
			"blight" => Img.Icon_Blight,
			"beast" => Img.Icon_Beast,
			"fear" => Img.Icon_Fear,
			"wilds" => Img.Icon_Wilds,
			"fast" => Img.Icon_Fast,
			"presence" => Img.Icon_Presence,
			"slow" => Img.Icon_Slow,
			"disease" => Img.Icon_Disease,
			"vitality" => Img.Icon_Vitality,
			"quake" => Img.Icon_Quake,
			"strife" => Img.Icon_Strife,
			"badlands" => Img.Icon_Badlands,
			"destroyedpresence" => Img.Icon_DestroyedPresence,
			"or-curly-before" => Img.OrCurlyBefore,
			"or-curly-after" => Img.OrCurlyAfter,
			"incarna" => Img.Icon_Incarna,
			"endless-dark" => Img.Icon_EndlessDark,
			"cardplay" => Img.CardPlay,
			"impending" => Img.ImpendingCard,	
			"wetland" => Img.Icon_Wetland,
			"jungle" => Img.Icon_Jungle,
			"sands" => Img.Icon_Sands,
			"mountain" => Img.Icon_Mountain,
			_ => Other( token ),
		};
	}

	static Img Other(string token){
		try{
			return ElementStrings.ParseEl( token ).GetIconImg();
		} catch( Exception ){
			return Img.None;
		}
	}

	readonly Dictionary<Img, Size> iconSizes = new Dictionary<Img, Size>();

}


