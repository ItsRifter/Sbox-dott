using Sandbox;

namespace dott
{
	[Library( "dott_pistol", Title = "Pistol" )]
	partial class Pistol : WeaponBase
	{
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		public override float PrimaryRate => 15.0f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.0f;
		public override int ClipSize => 16;
		
		public override int Bucket => 1;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
			AmmoClip = 16;
		}

		public override bool CanPrimaryAttack()
		{
			var tr = Trace.Ray( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward * 1024*2)
				.UseHitboxes()
				.Ignore( Owner )
				.Size( 5 )
				.Run();

			if ( tr.Entity is PlayerBase target )
			{
				if ( target.CurTeam == teams.Survivor)
					return false;
			}

			return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			if ( !TakeAmmo( 1 ) )
			{
				DryFire();
				return;
			}
			
			ShootEffects();
			PlaySound( "rust_pistol.shoot" );

			ShootBullet( 0.05f, 1.5f, 15.0f, 3.0f );

		}
	}

}
