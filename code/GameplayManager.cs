
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



namespace dott
{
	
	[Library( "dott" )]
	public partial class GameplayManager : Sandbox.Game
	{
		private TimeSince timer;
		
		[Net] 
		public static RoundState curState { get; set; }
		
		public GameplayManager()
		{
			if ( IsServer )
			{
				new Hud();
				curState = RoundState.Idle;
			}
		}

		public void BeginGame()
		{
			if ( curState != RoundState.Idle )
				return;
			
			RespawnAllHumans();
			curState = RoundState.Start;
			
		}

		public void RestartGame()
		{
			if ( curState != RoundState.Post )
				return;
			
			RespawnAllHumans();
			curState = RoundState.Start;
		}

		public void InfectRandomHuman()
		{
			var randHuman = GetAllHumans();
			int randomInt = Rand.Int(0, randHuman.Count - 1 );

			while ( randHuman[randomInt] == null )
			{
				randomInt = Rand.Int(0, randHuman.Count - 1 );
			}
			
			randHuman[randomInt].SetTeam( teams.Undead );
			randHuman[randomInt].Respawn();

			curState = RoundState.Active;
			
			
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

		[Event.Tick]
		public void UpdateTime()
		{
			if ( timer > 10 && Client.All.Count > 1 && curState == RoundState.Start)
				InfectRandomHuman();

			if ( timer > 6 && curState == RoundState.Post )
				RestartGame();


		}
		
		public static void CheckRoundStatus()
		{
			var humans = GetAllHumans();

			for ( int i = 0; i < humans.Count; i++ )
			{
				Log.Info(humans[i].ToString());
			}
			
			if ( humans.Count > 0 )
				return;

			Log.Info( ("EVERYONES DEAD") );
			Event.Run("evnt_endgame" );
		}

		[Event("evnt_endgame")]
		public void EndGame()
		{
			curState = RoundState.Post;
			timer = 0;
		}
		
		public void StopGame()
		{
			curState = RoundState.Idle;
		}
		
		public static RoundState GetState()
		{
			return curState;
		}
		
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new PlayerBase();
			client.Pawn = player;
			
			player.InitialSpawn();

			if ( Client.All.Count > 1 && curState == RoundState.Idle)
			{
				BeginGame();
				
			}
			
			timer = 0;
		}
		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			base.ClientDisconnect(cl, reason);
			
			if ( Client.All.Count < 2 && curState != RoundState.Idle)
			{
				StopGame();
			} 
		}
	}

}
