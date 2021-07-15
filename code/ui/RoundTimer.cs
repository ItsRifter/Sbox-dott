using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

partial class RoundTimer : Panel
{
	public static Label timeleft;
	public static float TotalTimeLeft;
	private int nextThink;
	public RoundTimer() { 
		StyleSheet.Load( "/ui/RoundTimer.scss" );

		timeleft = Add.Label( "Time left: ", "timing" );
	}
}
