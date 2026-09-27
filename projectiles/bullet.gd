class_name Bullet
extends Area2D

@export var move_speed: float = 300.0
@export var lifetime: float = 5.0
var is_consumed: bool = false

func _ready() -> void:
	get_tree().create_timer(lifetime).timeout.connect(queue_free)

func _physics_process(delta: float) -> void:
	if not is_consumed:
		position.x += move_speed * delta

func consume() -> bool:
	if is_consumed:
		return false
	is_consumed = true
	queue_free()
	return true
