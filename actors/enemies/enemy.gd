class_name Enemy
extends Area2D

signal defeated

@export var move_speed: float = 50.0
@export var animator: AnimatedSprite2D
var is_dead: bool = false

func _physics_process(delta: float) -> void:
	if not is_dead:
		position.x -= move_speed * delta
	if position.x < -240.0:
		queue_free()

func _on_body_entered(body: Node2D) -> void:
	if not is_dead and body is Player:
		body.die()

func _on_area_entered(area: Area2D) -> void:
	if is_dead or not area is Bullet:
		return
	if not area.consume():
		return
	is_dead = true
	set_deferred("monitoring", false)
	defeated.emit()
	animator.play("death")
	$Death.play()
	get_tree().create_timer(0.6).timeout.connect(queue_free)
