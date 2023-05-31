using Sandbox;

namespace MyGame;

public partial class WeaponViewModel : BaseViewModel
{
	protected Weapon Weapon { get; init; }

	public WeaponViewModel( Weapon weapon )
	{
		Weapon = weapon;
		EnableShadowCasting = false;
		EnableViewmodelRendering = true;
	}

	public override void PlaceViewmodel()
	{
		base.PlaceViewmodel();

		Camera.Main.SetViewModelCamera( 80f, 1, 500 );
	}
}
