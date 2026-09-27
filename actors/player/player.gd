class_name Player
extends CharacterBody2D

signal shot_requested(global_position: Vector2)
signal died

@export var move_speed: float = 100.0
@export var shot_interval: float = 1.0
@export var animator: AnimatedSprite2D
var is_dead: bool = false

@onready var shot_timer: Timer = $ShotTimer
@onready var run_sound: AudioStreamPlayer2D = $Run

func _ready() -> void:
	shot_timer.start(shot_interval)

func _physics_process(_delta: float) -> void:
	if is_dead:
		return
	velocity = Input.get_vector("left", "right", "up", "down") * move_speed
	if velocity == Vector2.ZERO:
		animator.play("idle")
		run_sound.stop()
	else:
		animator.play("run")
		if not run_sound.playing:
			run_sound.play()
	move_and_slide()

func die() -> void:
	if is_dead:
		return
	is_dead = true
	velocity = Vector2.ZERO
	shot_timer.stop()
	run_sound.stop()
	died.emit()
	animator.play("game_over")
	$GameOverSound.play()

func _on_fire() -> void:
	if is_dead or velocity != Vector2.ZERO:
		return
	$Fire.play()
	shot_requested.emit(global_position + Vector2(12, 6))
