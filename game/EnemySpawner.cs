using Godot;

namespace Demo02;

[GlobalClass]
public partial class EnemySpawner : Node
{
	[Signal]
	public delegate void EnemySpawnedEventHandler(Enemy enemy);

	[Export] public PackedScene EnemyScene { get; set; }
	[Export] public Node2D World { get; set; }
	[Export] public float InitialInterval { get; set; } = 3.0f;
	[Export] public float MinimumInterval { get; set; } = 1.0f;
	[Export] public float Acceleration { get; set; } = 0.2f;
	[Export] public float SpawnX { get; set; } = 265.0f;
	[Export] public Vector2 SpawnYRange { get; set; } = new Vector2(40, 100);

	public bool IsRunning { get; private set; }
	public Timer SpawnTimer { get; private set; }

	public override void _Ready()
	{
		SpawnTimer = GetNode<Timer>("SpawnTimer");
		SpawnTimer.Timeout += OnSpawnTimeout;
	}

	public void Start()
	{
		IsRunning = true;
		SpawnTimer.Start(InitialInterval);
	}

	public void Stop()
	{
		IsRunning = false;
		SpawnTimer.Stop();
	}

	public override void _Process(double delta)
	{
		if (!IsRunning)
		{
			return;
		}

		SpawnTimer.WaitTime = Mathf.Clamp(
			(float)SpawnTimer.WaitTime - Acceleration * (float)delta, MinimumInterval, InitialInterval);
	}

	public void OnSpawnTimeout()
	{
		if (!IsRunning)
		{
			return;
		}

		var enemy = EnemyScene.Instantiate<Enemy>();
		enemy.Position = new Vector2(SpawnX, (float)GD.RandRange(SpawnYRange.X, SpawnYRange.Y));
		EmitSignal(SignalName.EnemySpawned, enemy);
		World.AddChild(enemy);
	}
}
