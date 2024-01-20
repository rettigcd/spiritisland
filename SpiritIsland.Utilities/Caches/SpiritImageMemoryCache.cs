using System.Drawing;

namespace SpiritIsland;

/// <summary>
/// Initializes Spirit tokens/markers and caches them in memory.
/// </summary>
public class SpiritImageMemoryCache {

	public SpiritImageMemoryCache( ResourceImages images) {
		_tokenImages = new Dictionary<ISpaceEntity, Image> {
			[Token.Blight] = images.GetImg( Img.Blight ),
			[Token.Beast] = images.GetImg( Img.Beast ),
			[Token.Wilds] = images.GetImg( Img.Wilds ),
			// [Token.Disease] = images.GetImage( Img.Disease ),
			[Token.Badlands] = images.GetImg( Img.Badlands ),
			[Token.Vitality] = images.GetImg( Img.Vitality ),
		};
		_strife = images.GetImg(Img.Strife);
		_images = images;

		_fearTokenImage = images.GetImg(Img.Fear);
		_grayFear = ModCache.Grays(images).GetImage(Img.Fear);

	}

	public Dictionary<ISpaceEntity, Image> _tokenImages; // because we need different images for different damaged invaders.
	public Image? _presenceImg => _spiritToken != null ? _tokenImages[_spiritToken] : null;
	IToken? _spiritToken;
	public Img _incarnaImg;
	public Image _strife;
	public Image _fearTokenImage;
	public Image _grayFear;


	public void InitNewSpirit( Spirit spirit ) {
		DisposeOldSpirit();

		var resourceImages = ResourceImages.Singleton;

		// Load them
		_spiritToken = spirit.Presence.Token;
		_tokenImages[_spiritToken] = resourceImages.GetSpiritMarker( spirit, Img.Token_Presence );
		_tokenImages[Token.Defend]  = resourceImages.GetSpiritMarker( spirit, Img.Defend );
		_tokenImages[Token.Isolate] = resourceImages.GetSpiritMarker( spirit, Img.Isolate );
	}

	public Image AccessTokenImage( IToken imageToken ) {

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
			Image image = _images.GetImg( element.GetTokenImg() );
			_elementImages.Add( element, image );
		}
		return _elementImages[element];
	}

	#region private

	readonly Dictionary<Element, Image> _elementImages = new();
	readonly ResourceImages _images;

	void DisposeOldSpirit() {
		DisposeTokenImage( _spiritToken );
		DisposeTokenImage( Token.Defend );
		DisposeTokenImage( Token.Isolate );
	}
	void DisposeTokenImage( ISpaceEntity? x ) {
		if( x is not null && _tokenImages.ContainsKey( x ))
			_tokenImages[x]?.Dispose();
	}

	#endregion
}
