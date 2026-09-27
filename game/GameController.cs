using Godot;

namespace Demo02;

[GlobalClass]
public partial class GameController : Node2D
{
	[Export] public PackedScene BulletScene { get; set; }

	public int Score { get; private set; }
	public bool IsRunning { get; private set; } = true;

	public Node2D World { get; private set; }
	public Player Player { get; private set; }
	public EnemySpawner Spawner { get; private set; }
	public HUD Hud { get; private set; }
	public Timer RestartTimer { get; private set; }

	public override void _Ready()
	{
		World = GetNode<Node2D>("World");
		Player = GetNode<Player>("World/Player");
		Spawner = GetNode<EnemySpawner>("EnemySpawner");
		Hud = GetNode<HUD>("HUD");
		RestartTimer = GetNode<Timer>("RestartTimer");

		Player.ShotRequested += OnShotRequested;
		Player.Died += OnPlayerDied;
		Spawner.EnemySpawned += OnEnemySpawned;
		RestartTimer.Timeout += OnRestartTimeout;

		Hud.SetScore(Score);
		Spawner.Start();
	}

	private void OnShotRequested(Vector2 spawnPosition)
	{
		if (!IsRunning)
		{
			return;
		}

		var bullet = BulletScene.Instantiate<Bullet>();
		World.AddChild(bullet);
		bullet.GlobalPosition = spawnPosition;
	}

	private void OnEnemySpawned(Enemy enemy)
	{
		enemy.Defeated += OnEnemyDefeated;
	}

	private void OnEnemyDefeated()
	{
		if (!IsRunning)
		{
			return;
		}

		Score++;
		Hud.SetScore(Score);
	}

	private void OnPlayerDied()
	{
		if (!IsRunning)
		{
			return;
		}

		IsRunning = false;
		Spawner.Stop();
		Hud.ShowGameOver();
		RestartTimer.Start();
	}

	private void OnRestartTimeout()
	{
		GetTree().ReloadCurrentScene();
	}
}
