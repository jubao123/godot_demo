class_name EnemySpawner
extends Node

signal enemy_spawned(enemy: Enemy)

@export var enemy_scene: PackedScene
@export var world: Node2D
@export var initial_interval: float = 3.0
@export var minimum_interval: float = 1.0
@export var acceleration: float = 0.2
@export var spawn_x: float = 265.0
@export var spawn_y_range: Vector2 = Vector2(40, 100)
var is_running: bool = false

@onready var spawn_timer: Timer = $SpawnTimer

func _ready() -> void:
	spawn_timer.timeout.connect(_on_spawn_timeout)

func start() -> void:
	is_running = true
	spawn_timer.start(initial_interval)

func stop() -> void:
	is_running = false
	spawn_timer.stop()

func _process(delta: float) -> void:
	if is_running:
		spawn_timer.wait_time = clampf(
			spawn_timer.wait_time - acceleration * delta, minimum_interval, initial_interval
		)

func _on_spawn_timeout() -> void:
	if not is_running:
		return
	var enemy: Enemy = enemy_scene.instantiate() as Enemy
	enemy.position = Vector2(spawn_x, randf_range(spawn_y_range.x, spawn_y_range.y))
	enemy_spawned.emit(enemy)
	world.add_child(enemy)
