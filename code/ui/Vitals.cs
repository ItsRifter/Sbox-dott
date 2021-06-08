﻿
using dott;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Vitals : Panel
{
	public Label Health;
	public Label Team;
	
	public Vitals()
	{
		Health = Add.Label( "100", "health" );
		Team = Add.Label( "null", "team" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		Health.Text = $"{player.Health.CeilToInt()}";
		Health.SetClass( "danger", player.Health < 40.0f );

		if ( player is PlayerBase ply )
		{
			Team.Text = $" - {ply.CurTeam}";
		}
	}
}
