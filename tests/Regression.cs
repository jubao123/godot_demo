using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo02;

[GlobalClass]
public partial class Regression : SceneTree
{
	private static readonly PackedScene PlayerScene = GD.Load<PackedScene>("res://actors/player/player.tscn");
	private static readonly PackedScene EnemyScene = GD.Load<PackedScene>("res://actors/enemies/slime.tscn");
	private static readonly PackedScene BulletScene = GD.Load<PackedScene>("res://projectiles/bullet.tscn");

	private int _failures;
	private int _defeats;
	private int _deaths;
	private readonly List<Vector2> _shots = new();
	private int _spawns;

	public override void _Initialize()
	{
		_ = RunAsync();
	}

	private void Check(bool condition, string message)
	{
		if (!condition)
		{
			_failures++;
			GD.PushError(message);
		}
	}

	private async Task RunAsync()
	{
		try
		{
			await ToSignal(this, SceneTree.SignalName.ProcessFrame);

			var sandbox = new Node2D();
			Root.AddChild(sandbox);
			sandbox.Position = new Vector2(50, 25);

			var player = PlayerScene.Instantiate<Player>();
			var enemy = EnemyScene.Instantiate<Enemy>();
			var otherEnemy = EnemyScene.Instantiate<Enemy>();
			var bullet = BulletScene.Instantiate<Bullet>();
			foreach (Node actor in new Node[] { player, enemy, otherEnemy, bullet })
			{
				sandbox.AddChild(actor);
			}

			player.Died += () => _deaths++;
			player.ShotRequested += point => _shots.Add(point);
			enemy.Defeated += () => _defeats++;
			otherEnemy.Defeated += () => _defeats++;

			player.OnFire();
			Check(_shots.Count == 1 && _shots[0] == player.GlobalPosition + new Vector2(12, 6),
				"Stationary shot uses global coordinates");
			player.Velocity = Vector2.Right;
			player.OnFire();
			Check(_shots.Count == 1, "Moving player does not shoot");

			enemy.OnAreaEntered(bullet);
			enemy.OnAreaEntered(bullet);
			otherEnemy.OnAreaEntered(bullet);
			Check(_defeats == 1 && enemy.IsDead && !otherEnemy.IsDead, "One bullet defeats only one enemy");
			Check(!bullet.Consume(), "Consumed bullet rejects a second consumption");

			var extraBullet = BulletScene.Instantiate<Bullet>();
			sandbox.AddChild(extraBullet);
			enemy.OnAreaEntered(extraBullet);
			Check(!extraBullet.IsConsumed && _defeats == 1, "Dead enemy ignores further bullets");

			otherEnemy.OnBodyEntered(player);
			player.Die();
			player.OnFire();
			Check(_deaths == 1 && player.IsDead && _shots.Count == 1, "Death is emitted once and stops firing");
			Check(player.ShotTimer.IsStopped(), "Death stops shot timer");

			sandbox.QueueFree();
			await ToSignal(this, SceneTree.SignalName.ProcessFrame);
			await CheckPhysicsAsync();

			ChangeSceneToFile("res://game/game.tscn");
			await ToSignal(this, SceneTree.SignalName.SceneChanged);
			var game = CurrentScene as GameController;
			Check(game.Score == 0 && game.IsRunning && !game.Player.IsDead, "Fresh game starts with initial state");
			Check(game.World.YSortEnabled && game.Player.GetParent() == game.World, "Actors share a Y-sorted world");

			game.Spawner.EnemySpawned += _ => _spawns++;
			game.Spawner._Process(20.0);
			Check(Mathf.IsEqualApprox((float)game.Spawner.SpawnTimer.WaitTime, 1.0f),
				"Spawn acceleration clamps to minimum");
			game.Spawner.OnSpawnTimeout();
			var spawnedEnemy = game.World.GetChild(game.World.GetChildCount() - 1) as Enemy;
			Check(_spawns == 1 && spawnedEnemy.Position.X == 265.0f
					&& spawnedEnemy.Position.Y >= 40.0f && spawnedEnemy.Position.Y <= 100.0f,
				"Spawner places enemy in configured range");

			game.Player.OnFire();
			var spawnedBullet = game.World.GetChild(game.World.GetChildCount() - 1) as Bullet;
			Check(spawnedBullet != null && spawnedBullet.GlobalPosition == game.Player.GlobalPosition + new Vector2(12, 6),
				"Controller places bullet in world");

			spawnedEnemy.OnAreaEntered(spawnedBullet);
			Check(game.Score == 1 && game.Hud.ScoreLabel.Text == "Score:1",
				"Defeated signal updates score and HUD");

			game.Spawner.OnSpawnTimeout();
			var survivor = game.World.GetChild(game.World.GetChildCount() - 1) as Enemy;
			game.Player.Die();
			Check(!game.IsRunning && !game.Spawner.IsRunning && game.Spawner.SpawnTimer.IsStopped(),
				"Death immediately stops spawning");
			Check(game.Hud.GameOverLabel.Visible && Mathf.IsEqualApprox((float)game.RestartTimer.WaitTime, 3.0f),
				"Game over displays and schedules three-second restart");

			var previousSpawns = _spawns;
			game.Spawner.OnSpawnTimeout();
			Check(_spawns == previousSpawns, "Late spawn callback is ignored");

			var lateBullet = BulletScene.Instantiate<Bullet>();
			game.World.AddChild(lateBullet);
			survivor.OnAreaEntered(lateBullet);
			Check(survivor.IsDead && game.Score == 1, "Enemies finish death after game over without scoring");

			var oldGameId = game.GetInstanceId();
			await ToSignal(CreateTimer(2.7), SceneTreeTimer.SignalName.Timeout);
			Check(GodotObject.IsInstanceValid(game) && game.GetInstanceId() == oldGameId, "Restart does not occur early");
			await ToSignal(CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);

			var restarted = CurrentScene as GameController;
			Check(restarted != null && restarted.GetInstanceId() != oldGameId, "Restart timer reloads the scene");
			Check(restarted.Score == 0 && restarted.IsRunning && !restarted.Player.IsDead, "Restart resets score and player");
			Check(restarted.Spawner.SpawnTimer.WaitTime > 2.8 && restarted.Spawner.IsRunning,
				"Restart restores spawn interval");
			Check(!restarted.Hud.GameOverLabel.Visible && !restarted.Player.ShotTimer.IsStopped(),
				"Restart resets HUD and firing");

			Root.GetNode<AudioStreamPlayer2D>("Bgm").Stop();
			restarted.QueueFree();
			await ToSignal(CreateTimer(0.1), SceneTreeTimer.SignalName.Timeout);

			GD.Print($"Regression checks completed: {_failures} failures");
			Quit(_failures == 0 ? 0 : 1);
		}
		catch (Exception e)
		{
			GD.PushError("Regression harness exception: " + e);
			Quit(1);
		}
	}

	private async Task CheckPhysicsAsync()
	{
		var sandbox = new Node2D();
		Root.AddChild(sandbox);

		var player = PlayerScene.Instantiate<Player>();
		sandbox.AddChild(player);
		player.ShotTimer.Stop();

		var actions = new (StringName Action, Vector2 Direction)[]
		{
			(new StringName("left"), Vector2.Left),
			(new StringName("right"), Vector2.Right),
			(new StringName("up"), Vector2.Up),
			(new StringName("down"), Vector2.Down),
		};
		foreach (var (action, direction) in actions)
		{
			var before = player.Position;
			Input.ActionPress(action);
			await ToSignal(CreateTimer(0.08), SceneTreeTimer.SignalName.Timeout);
			Input.ActionRelease(action);
			var movement = player.Position - before;
			Check(movement.Dot(direction) > 0.0f, "Input action moves player: " + action);
		}

		player.Position = new Vector2(-100, -100);
		var enemy = EnemyScene.Instantiate<Enemy>();
		enemy.Position = new Vector2(60, 0);
		sandbox.AddChild(enemy);

		var bullet = BulletScene.Instantiate<Bullet>();
		bullet.Position = new Vector2(0, 9);
		sandbox.AddChild(bullet);
		await ToSignal(CreateTimer(0.25), SceneTreeTimer.SignalName.Timeout);
		Check(enemy.IsDead && !GodotObject.IsInstanceValid(bullet),
			"Physics collision defeats enemy and removes bullet");

		await ToSignal(CreateTimer(0.65), SceneTreeTimer.SignalName.Timeout);
		Check(!GodotObject.IsInstanceValid(enemy), "Death presentation cleans up enemy");

		var expiring = BulletScene.Instantiate<Bullet>();
		expiring.Lifetime = 0.05f;
		sandbox.AddChild(expiring);
		await ToSignal(CreateTimer(0.1), SceneTreeTimer.SignalName.Timeout);
		Check(!GodotObject.IsInstanceValid(expiring), "Bullet expires without a hit");

		var attacker = EnemyScene.Instantiate<Enemy>();
		attacker.Position = player.Position;
		sandbox.AddChild(attacker);
		await ToSignal(CreateTimer(0.1), SceneTreeTimer.SignalName.Timeout);
		Check(player.IsDead, "Physics body collision kills player");

		sandbox.QueueFree();
		await ToSignal(this, SceneTree.SignalName.ProcessFrame);
	}
}
