﻿
using dott;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Vitals : Panel
{
	public Label Health;
	public Label Team;
	
	public Vitals() { 
		StyleSheet.Load( "/ui/Vitals.scss" ); 
		Health = Add.Label( "100", "health" );
		Team = Add.Label( "null", "team" );
	}

	[ Event("client.tick") ]
	public void Tick() {
		var player = Local.Pawn;
		if ( player == null ) return;

		Health.Text = $"{player.Health.CeilToInt()}";
		Health.SetClass( "danger", player.Health < 40.0f );

		if ( player is PlayerBase ply ) {
			var k = ply.CurTeam.ToString();
			if ( k.Equals( "Undead" ) ) {
				if (!HasClass( "undead" )) {
					AddClass( "undead" );
				}
				Team.Text = k.ToString() + "🤢";
			}
			
			if ( k.Equals( "Survivor" ) ) {
				RemoveClass( "undead" );
				Team.Text = k.ToString();
			}
			
		}
	}
}
