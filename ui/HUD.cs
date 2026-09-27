using Godot;

namespace Demo02;

[GlobalClass]
public partial class HUD : CanvasLayer
{
	public Label ScoreLabel { get; private set; }
	public Label GameOverLabel { get; private set; }

	public override void _Ready()
	{
		ScoreLabel = GetNode<Label>("Label");
		GameOverLabel = GetNode<Label>("GameOverLabel");
	}

	public void SetScore(int value)
	{
		ScoreLabel.Text = "Score:" + value;
	}

	public void ShowGameOver()
	{
		GameOverLabel.Show();
	}
}
