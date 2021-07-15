using Sandbox;
using System;
using System.Linq;

public partial class PlayerBase : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;

	private DamageInfo lastDamage;

	[Net] 
	public teams CurTeam { get; set; }
	
	public ICamera LastCamera { get; set; }
	
	public PlayerBase()
	{
		Inventory = new Inventory( this );
	}
	
	
	public void InitialSpawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		if ( GameplayManager.CurState != GameplayManager.RoundState.Active )
		{
			this.SetTeam(teams.Survivor);
		}
		else
		{
			this.SetTeam(teams.Undead);
			GiveWeapons();
		}
		
		
		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();
		Camera = new FirstPersonCamera();
		
		Dress();
		
		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		
		base.Respawn();
		
		if (GameplayManager.CurState != GameplayManager.RoundState.Active )
			return;
		else if (GameplayManager.CurState == GameplayManager.RoundState.Active)
			SetTeam( teams.Undead );
	}

	public void GiveWeapons()
	{
		this.Inventory.DeleteContents();
		if ( this.CurTeam == teams.Survivor )
		{
			switch ( Rand.Int(1, 3) )
            {
                case 1:
                	Inventory.Add( new Pistol(), true );
                	GiveAmmo( AmmoType.Pistol, 120 );
                	break;
                
                case 2:
                	Inventory.Add( new Shotgun(), true );
                	GiveAmmo( AmmoType.Buckshot, 64 );
                	break;
                
                case 3:
                	Inventory.Add( new SMG(), true );
                	GiveAmmo( AmmoType.SMG, 210 );
                	break;
            }
		}
		else
			Inventory.Add( new Claws(), true );
	}
	
	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );
		
		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();
		Camera = new FirstPersonCamera();
		
		Dress();
		ClearAmmo();
		
		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		base.Respawn();
		
		GiveWeapons();
	}
	
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		SimulateActiveChild( cl, ActiveChild );
	}
	
	public void SetTeam(teams targetTeam) 
	{
		this.CurTeam = targetTeam;
		
		if ( targetTeam == teams.Undead )
		{
			this.RenderColor = new Color32( 86, 227, 54 );
		}
		else
		{
			this.RenderColor = new Color32( 255, 255, 255 );
		}
		
		using ( Prediction.Off() )
        {
            SetTeamOnClient(To.Single(this), targetTeam);
        }
		
		GameplayManager.CheckRoundStatus();
	}

	[ClientRpc]
	public void SetTeamOnClient(teams targetTeam)
	{
		this.CurTeam = targetTeam;
	}
	
	public override void TakeDamage( DamageInfo info )
	{
		var attacker = info.Attacker as PlayerBase;

		if ( GameplayManager.CurState != GameplayManager.RoundState.Active )
			return;
		
		if ( attacker.IsValid() && attacker.CurTeam == this.CurTeam  )
			return;
		
		base.TakeDamage(info);

	}
	
	public override void OnKilled()
	{
		base.OnKilled();
		
		Inventory.DeleteContents();
		
		BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );
		
		if(this.CurTeam != teams.Undead)
		{
			SetTeam(teams.Undead);
		}

		GameplayManager.CheckRoundStatus();
		Camera = new SpectateRagdollCamera();
		EnableDrawing = false;
		
	}
}
