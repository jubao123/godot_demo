using Godot;

namespace Demo02;

[GlobalClass]
public partial class Enemy : Area2D
{
	[Signal]
	public delegate void DefeatedEventHandler();

	[Export] public float MoveSpeed { get; set; } = 50.0f;
	[Export] public AnimatedSprite2D Animator { get; set; }
	public bool IsDead { get; private set; }

	public override void _PhysicsProcess(double delta)
	{
		if (!IsDead)
		{
			Position += new Vector2(-MoveSpeed * (float)delta, 0f);
		}

		if (Position.X < -240.0f)
		{
			QueueFree();
		}
	}

	public void OnBodyEntered(Node2D body)
	{
		if (!IsDead && body is Player player)
		{
			player.Die();
		}
	}

	public void OnAreaEntered(Area2D area)
	{
		if (IsDead || area is not Bullet bullet)
		{
			return;
		}

		if (!bullet.Consume())
		{
			return;
		}

		IsDead = true;
		SetDeferred(Area2D.PropertyName.Monitoring, false);
		EmitSignal(SignalName.Defeated);
		Animator.Play("death");
		GetNode<AudioStreamPlayer2D>("Death").Play();
		GetTree().CreateTimer(0.6).Timeout += QueueFree;
	}
}
