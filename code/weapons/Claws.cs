﻿using Sandbox;

namespace dott
{
	[Library( "dott_claws", Title = "Claws" )]
	partial class Claws : WeaponBase
	{
		public override float PrimaryRate => 6.0f;
		public override float SecondaryRate => 0.0f;
		public override float ReloadTime => 0.0f;
		public override int ClipSize => 99;
		
		public override int Bucket => 1;

		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = 99;
		}
		
		public override bool CanPrimaryAttack()
		{
			return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
		}
		
		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 0 );
			anim.SetParam( "aimat_weight", 1.0f );
		}
		
		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;
			
			ShootBullet( 0.0f, 1f, 25.0f, 1.0f );
		}
	}
}
