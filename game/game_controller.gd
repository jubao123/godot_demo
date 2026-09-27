class_name GameController
extends Node2D

@export var bullet_scene: PackedScene
var score: int = 0
var is_running: bool = true

@onready var world: Node2D = $World
@onready var player: Player = $World/Player
@onready var spawner: EnemySpawner = $EnemySpawner
@onready var hud: HUD = $HUD
@onready var restart_timer: Timer = $RestartTimer

func _ready() -> void:
	player.shot_requested.connect(_on_shot_requested)
	player.died.connect(_on_player_died)
	spawner.enemy_spawned.connect(_on_enemy_spawned)
	restart_timer.timeout.connect(_on_restart_timeout)
	hud.set_score(score)
	spawner.start()

func _on_shot_requested(spawn_position: Vector2) -> void:
	if not is_running:
		return
	var bullet: Bullet = bullet_scene.instantiate() as Bullet
	world.add_child(bullet)
	bullet.global_position = spawn_position

func _on_enemy_spawned(enemy: Enemy) -> void:
	enemy.defeated.connect(_on_enemy_defeated)

func _on_enemy_defeated() -> void:
	if not is_running:
		return
	score += 1
	hud.set_score(score)

func _on_player_died() -> void:
	if not is_running:
		return
	is_running = false
	spawner.stop()
	hud.show_game_over()
	restart_timer.start()

func _on_restart_timeout() -> void:
	get_tree().reload_current_scene()
