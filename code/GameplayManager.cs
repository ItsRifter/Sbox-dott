
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sandbox.Rcon;
using Sandbox.UI;
using Entity = Sandbox.Entity;
using Random = Sandbox.ScreenShake.Random;



	
[Library( "dott" )]
public partial class GameplayManager : Sandbox.Game
{
	
	[Net]
	public static float TimeCurLeft { get; set; }

	[Net]
	public static RoundState CurState { get; set; } = RoundState.Idle;
	
	private int nextThink;
	
	public enum RoundState
	{
		Idle,
		Start,
		Active,
		Post 
	}
	
	public GameplayManager()
	{
		if ( IsServer )
		{
			new Hud();
		}
	}
	public void BeginGame()
	{
		RespawnAllHumans();
		CurState = RoundState.Start;
		TimeCurLeft = 10f;
		
		using ( Prediction.Off() )
		{
			SetTimer(To.Everyone, 10f, RoundState.Start);
		}
	}

	public void StopGame()
	{
		CurState = RoundState.Idle;
		
		using ( Prediction.Off() )
		{
			DeactivateTimer(To.Everyone);
		}
	}

	public void RestartGame()
	{
		TimeCurLeft = 10f;
		RespawnAllHumans();
		CurState = RoundState.Start;
		
		using ( Prediction.Off() )
		{
			SetTimer(To.Everyone, 10f, RoundState.Start);
		}
	}

	[Event.Tick]
	public void UpdateTime()
	{
		if ( Host.IsClient )
			return;
		
		if ( nextThink <= 0f )
		{
			TimeCurLeft -= 1;
			nextThink = 60;
		} else
			nextThink--;
		
		if ( TimeCurLeft <= 0f && CurState == RoundState.Start )
		{
			InfectRandomHuman();
			return;
		}

		if ( TimeCurLeft <= 0f && CurState == RoundState.Active )
		{
			CurState = RoundState.Post;
			TimeCurLeft = 5f;
			return;
		}
		
		if ( TimeCurLeft <= 0f && CurState == RoundState.Post )
		{
			RestartGame();
		}
		
		using ( Prediction.Off() )
		{
			SetTimer(To.Everyone, TimeCurLeft, CurState);
		}
	}
	
	public static void InfectRandomHuman()
	{
		var randHuman = GetAllHumans();
		int randomInt = Rand.Int(0, randHuman.Count - 1 );
		
		while ( randHuman[randomInt] == null )
		{
			randomInt = Rand.Int(0, randHuman.Count - 1 );
		}
		
		randHuman[randomInt].SetTeam( teams.Undead );
		randHuman[randomInt].Respawn();

		CurState = RoundState.Active;

		TimeCurLeft = 180f;

		using ( Prediction.Off() )
		{
			SetTimer(To.Everyone, TimeCurLeft, RoundState.Active);
		}
	}
	
	public void RespawnAllHumans()
	{
		foreach ( var player in Client.All )
		{
			if ( player.Pawn is PlayerBase ply )
			{
				ply.SetTeam(teams.Survivor);
				ply.Respawn();
			}
		}
	}
	
	public static List<PlayerBase> GetAllHumans()
	{
		var curHumans = new List<PlayerBase>();

		foreach ( var p in Entity.All.OfType<PlayerBase>() )
		{
			if (p.CurTeam == teams.Survivor)
				curHumans.Add(p);
		}

		return curHumans;
	}
	
	public static List<PlayerBase> GetAllZombies()
	{
		var curZombies = new List<PlayerBase>();

		foreach ( var p in Entity.All.OfType<PlayerBase>() )
		{
			if (p.CurTeam == teams.Undead)
				curZombies.Add(p);
		}

		return curZombies;
	}

	public static void CheckRoundStatus()
	{
		var humans = GetAllHumans();
		var zombies = GetAllZombies();

		if ( humans.Count > 0 )
			return;

		Event.Run("evnt_endgame" );
	}

	[Event("evnt_endgame")]
	public void EndGame()
	{
		CurState = RoundState.Post;

		using ( Prediction.Off() )
		{
			SetTimer(To.Everyone, 5f, RoundState.Post);
		}

		TimeCurLeft = 5f;
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var player = new PlayerBase();
		client.Pawn = player;
		
		player.InitialSpawn();
		
		if ( Client.All.Count > 1 && CurState == RoundState.Idle)
		{
			Log.Info("Beginning game" );
			BeginGame();
		}
	}
	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect(cl, reason);
		
		if ( Client.All.Count > 1 && CurState == RoundState.Idle)
		{
			Log.Info("Stopping game" );
			StopGame();
		}
		
		CheckRoundStatus();
	}
	
	[ClientRpc]
	public static void SetTimer(float TimeSet, RoundState NewState)
	{
		TimeCurLeft = TimeSet;
		CurState = NewState;
		if ( CurState == RoundState.Start )
			RoundTimer.timeleft.Text = "Infection in " + TimeCurLeft;
		else if (CurState == RoundState.Active)
			RoundTimer.timeleft.Text = "Time left " + TimeCurLeft;
		
		if (CurState == RoundState.Post && GetAllHumans().Count <= 0 )
			RoundTimer.timeleft.Text = "Undead wins";
		else if (CurState == RoundState.Post && GetAllHumans().Count > 0 )
			RoundTimer.timeleft.Text = "Humanity wins";
		
	}

	[ClientRpc]
	public static void DeactivateTimer()
	{
		TimeCurLeft = 0f;
		CurState = RoundState.Post;
	}
	
	[ Event("client.tick") ]
	public void PanelThink() 
	{
		var client = Local.Pawn;
		if ( client == null ) return;

		if ( TimeCurLeft <= 0f )
			return;
		
		if ( nextThink <= 0f )
		{
			TimeCurLeft -= 1;
			nextThink = 60;
		} else
			nextThink--;
		
	}
}
