using Godot;

namespace Demo02;

[GlobalClass]
public partial class Player : CharacterBody2D
{
	[Signal]
	public delegate void ShotRequestedEventHandler(Vector2 globalPosition);

	[Signal]
	public delegate void DiedEventHandler();

	[Export] public float MoveSpeed { get; set; } = 100.0f;
	[Export] public float ShotInterval { get; set; } = 1.0f;
	[Export] public AnimatedSprite2D Animator { get; set; }
	public bool IsDead { get; private set; }

	public Timer ShotTimer { get; private set; }
	private AudioStreamPlayer2D _runSound;

	public override void _Ready()
	{
		ShotTimer = GetNode<Timer>("ShotTimer");
		_runSound = GetNode<AudioStreamPlayer2D>("Run");
		ShotTimer.Start(ShotInterval);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsDead)
		{
			return;
		}

		Velocity = Input.GetVector("left", "right", "up", "down") * MoveSpeed;
		if (Velocity == Vector2.Zero)
		{
			Animator.Play("idle");
			_runSound.Stop();
		}
		else
		{
			Animator.Play("run");
			if (!_runSound.Playing)
			{
				_runSound.Play();
			}
		}

		MoveAndSlide();
	}

	public void Die()
	{
		if (IsDead)
		{
			return;
		}

		IsDead = true;
		Velocity = Vector2.Zero;
		ShotTimer.Stop();
		_runSound.Stop();
		EmitSignal(SignalName.Died);
		Animator.Play("game_over");
		GetNode<AudioStreamPlayer2D>("GameOverSound").Play();
	}

	public void OnFire()
	{
		if (IsDead || Velocity != Vector2.Zero)
		{
			return;
		}

		GetNode<AudioStreamPlayer2D>("Fire").Play();
		EmitSignal(SignalName.ShotRequested, GlobalPosition + new Vector2(12, 6));
	}
}
