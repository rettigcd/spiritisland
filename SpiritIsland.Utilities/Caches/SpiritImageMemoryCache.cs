using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Initializes Spirit tokens/markers and caches them in memory.
/// </summary>
public class SpiritImageMemoryCache {

	public SpiritImageMemoryCache( ResourceImages images) {
		_tokenImages = new Dictionary<ISpaceEntity, Image> {
			[Token.Blight] = images.GetImage( Img.Blight ),
			[Token.Beast] = images.GetImage( Img.Beast ),
			[Token.Wilds] = images.GetImage( Img.Wilds ),
			// [Token.Disease] = images.GetImage( Img.Disease ),
			[Token.Badlands] = images.GetImage( Img.Badlands ),
			[Token.Vitality] = images.GetImage( Img.Vitality ),
		};
		_strife = images.Strife();
		_images = images;

		_fearTokenImage = images.Fear();
		_grayFear = images.FearGray();

	}

	public Dictionary<ISpaceEntity, Image> _tokenImages; // because we need different images for different damaged invaders.
	public Image? _presenceImg;
	public Img _incarnaImg;
	public Image _strife;
	public Image _fearTokenImage;
	public Image _grayFear;


	public void InitNewSpirit( PresenceTokenAppearance presenceAppearance ) {
		DisposeOldSpirit();

		// Load them
		_presenceImg = SpiritMarkerBuilder.BuildPresence( presenceAppearance );
		_tokenImages[Token.Defend] = SpiritMarkerBuilder.BuildMarker( Img.Defend, presenceAppearance.Adjustment );
		_tokenImages[Token.Isolate] = SpiritMarkerBuilder.BuildMarker( Img.Isolate, presenceAppearance.Adjustment );

	}

	public Image AccessTokenImage( IToken imageToken ) {
		if( imageToken is SpiritPresenceToken )
			return _presenceImg is not null ? _presenceImg : throw new Exception("missing presence image");

		// Invalidate Incarna Image when the .Img switches
		if( imageToken is Incarna it && it.Img != _incarnaImg ) {
			_incarnaImg = it.Img;
			if(_tokenImages.ContainsKey( imageToken )) {
				_tokenImages[imageToken].Dispose();
				_tokenImages.Remove(imageToken);
			}
		}

		if(!_tokenImages.ContainsKey( imageToken ))
			_tokenImages[imageToken] = ResourceImages.Singleton.GetTokenImage( imageToken );
		return _tokenImages[imageToken];
	}

	public Image GetElementImage( Element element ) {
		if(!_elementImages.ContainsKey( element )) {
			Image image = _images.GetImage( element.GetTokenImg() );
			_elementImages.Add( element, image );
		}
		return _elementImages[element];
	}

	#region private

	readonly Dictionary<Element, Image> _elementImages = new();
	readonly ResourceImages _images;

	void DisposeOldSpirit() {
		_presenceImg?.Dispose();
		if(_tokenImages.ContainsKey( Token.Defend ))
			_tokenImages[Token.Defend]?.Dispose();
		if(_tokenImages.ContainsKey( Token.Isolate ))
			_tokenImages[Token.Isolate]?.Dispose();
	}

	#endregion
}
