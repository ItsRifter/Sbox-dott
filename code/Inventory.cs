using Sandbox;
using System;
using System.Linq;

partial class Inventory : BaseInventory
{

	public Inventory( Player player ) : base( player )
	{

	}
	
	public override bool Add( Entity ent, bool makeActive = false )
	{
		var player = Owner as PlayerBase;
		var weapon = ent as WeaponBase;

		if ( weapon != null && IsCarryingType( ent.GetType() ) )
		{
			var ammo = weapon.AmmoClip;
			var ammoType = weapon.AmmoType;

			if ( ammo > 0 )
			{
				player.GiveAmmo( ammoType, ammo );
			}

			// Despawn it
			ent.Delete();
			return false;
		}

		return base.Add( ent, makeActive );
	}
	
	/*
	public virtual bool SetActiveSlot( int i, bool evenIfEmpty = false )
	{
		var ent = GetSlot( i );
		if ( Owner.ActiveChild == ent )
			return false;

		if ( !evenIfEmpty && ent == null )
			return false;

		Owner.ActiveChild = ent;
		return ent.IsValid();
	}
	*/

	public override bool SetActive( Entity ent )
	{
		if ( Active == ent ) return false;
		if ( !Contains( ent ) ) return false;

		Owner.ActiveChild = ent;
		return true;
	}
	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x.GetType() == t );
	}
}
