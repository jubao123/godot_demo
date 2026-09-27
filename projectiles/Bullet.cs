using Godot;

namespace Demo02;

[GlobalClass]
public partial class Bullet : Area2D
{
	[Export] public float MoveSpeed { get; set; } = 300.0f;
	[Export] public float Lifetime { get; set; } = 5.0f;
	public bool IsConsumed { get; private set; }

	public override void _Ready()
	{
		GetTree().CreateTimer(Lifetime).Timeout += QueueFree;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsConsumed)
		{
			Position += new Vector2(MoveSpeed * (float)delta, 0f);
		}
	}

	public bool Consume()
	{
		if (IsConsumed)
		{
			return false;
		}

		IsConsumed = true;
		QueueFree();
		return true;
	}
}
