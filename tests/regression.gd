extends SceneTree

const PLAYER_SCENE = preload("res://actors/player/player.tscn")
const ENEMY_SCENE = preload("res://actors/enemies/slime.tscn")
const BULLET_SCENE = preload("res://projectiles/bullet.tscn")

var failures: int = 0
var defeats: int = 0
var deaths: int = 0
var shots: Array[Vector2] = []
var spawns: int = 0

func _initialize() -> void:
	_run.call_deferred()

func check(condition: bool, message: String) -> void:
	if not condition:
		failures += 1
		push_error(message)

func _run() -> void:
	# A plain parent deliberately provides no game fields or methods.
	var sandbox := Node2D.new()
	root.add_child(sandbox)
	sandbox.position = Vector2(50, 25)
	var player: Player = PLAYER_SCENE.instantiate()
	var enemy: Enemy = ENEMY_SCENE.instantiate()
	var other_enemy: Enemy = ENEMY_SCENE.instantiate()
	var bullet: Bullet = BULLET_SCENE.instantiate()
	for actor: Node in [player, enemy, other_enemy, bullet]:
		sandbox.add_child(actor)
	player.died.connect(func() -> void: deaths += 1)
	player.shot_requested.connect(func(point: Vector2) -> void: shots.append(point))
	enemy.defeated.connect(func() -> void: defeats += 1)
	other_enemy.defeated.connect(func() -> void: defeats += 1)
	player._on_fire()
	check(shots.size() == 1 and shots[0] == player.global_position + Vector2(12, 6), "Stationary shot uses global coordinates")
	player.velocity = Vector2.RIGHT
	player._on_fire()
	check(shots.size() == 1, "Moving player does not shoot")
	enemy._on_area_entered(bullet)
	enemy._on_area_entered(bullet)
	other_enemy._on_area_entered(bullet)
	check(defeats == 1 and enemy.is_dead and not other_enemy.is_dead, "One bullet defeats only one enemy")
	check(not bullet.consume(), "Consumed bullet rejects a second consumption")
	var extra_bullet: Bullet = BULLET_SCENE.instantiate()
	sandbox.add_child(extra_bullet)
	enemy._on_area_entered(extra_bullet)
	check(not extra_bullet.is_consumed and defeats == 1, "Dead enemy ignores further bullets")
	other_enemy._on_body_entered(player)
	player.die()
	player._on_fire()
	check(deaths == 1 and player.is_dead and shots.size() == 1, "Death is emitted once and stops firing")
	check(player.shot_timer.is_stopped(), "Death stops shot timer")
	sandbox.queue_free()
	await process_frame
	await _check_physics()

	change_scene_to_file("res://game/game.tscn")
	await scene_changed
	var game: GameController = current_scene as GameController
	check(game.score == 0 and game.is_running and not game.player.is_dead, "Fresh game starts with initial state")
	check(game.world.y_sort_enabled and game.player.get_parent() == game.world, "Actors share a Y-sorted world")
	game.spawner.enemy_spawned.connect(func(_enemy: Enemy) -> void: spawns += 1)
	game.spawner._process(20.0)
	check(is_equal_approx(game.spawner.spawn_timer.wait_time, 1.0), "Spawn acceleration clamps to minimum")
	game.spawner._on_spawn_timeout()
	var spawned_enemy := game.world.get_child(game.world.get_child_count() - 1) as Enemy
	check(spawns == 1 and spawned_enemy.position.x == 265.0 and spawned_enemy.position.y >= 40.0 and spawned_enemy.position.y <= 100.0, "Spawner places enemy in configured range")
	game.player._on_fire()
	var spawned_bullet := game.world.get_child(game.world.get_child_count() - 1) as Bullet
	check(spawned_bullet != null and spawned_bullet.global_position == game.player.global_position + Vector2(12, 6), "Controller places bullet in world")
	spawned_enemy._on_area_entered(spawned_bullet)
	check(game.score == 1 and game.hud.score_label.text == "Score:1", "Defeated signal updates score and HUD")
	game.spawner._on_spawn_timeout()
	var survivor := game.world.get_child(game.world.get_child_count() - 1) as Enemy
	game.player.die()
	check(not game.is_running and not game.spawner.is_running and game.spawner.spawn_timer.is_stopped(), "Death immediately stops spawning")
	check(game.hud.game_over_label.visible and is_equal_approx(game.restart_timer.wait_time, 3.0), "Game over displays and schedules three-second restart")
	var previous_spawns := spawns
	game.spawner._on_spawn_timeout()
	check(spawns == previous_spawns, "Late spawn callback is ignored")
	var late_bullet: Bullet = BULLET_SCENE.instantiate()
	game.world.add_child(late_bullet)
	survivor._on_area_entered(late_bullet)
	check(survivor.is_dead and game.score == 1, "Enemies finish death after game over without scoring")
	var old_game_id := game.get_instance_id()
	await create_timer(2.7).timeout
	check(is_instance_valid(game) and game.get_instance_id() == old_game_id, "Restart does not occur early")
	await create_timer(0.5).timeout
	var restarted: GameController = current_scene as GameController
	check(restarted != null and restarted.get_instance_id() != old_game_id, "Restart timer reloads the scene")
	check(restarted.score == 0 and restarted.is_running and not restarted.player.is_dead, "Restart resets score and player")
	check(restarted.spawner.spawn_timer.wait_time > 2.8 and restarted.spawner.is_running, "Restart restores spawn interval")
	check(not restarted.hud.game_over_label.visible and not restarted.player.shot_timer.is_stopped(), "Restart resets HUD and firing")
	# Stop the persistent audio stream before headless engine teardown.
	(root.get_node("Bgm") as AudioStreamPlayer2D).stop()
	restarted.queue_free()
	await create_timer(0.1).timeout
	print("Regression checks completed: %d failures" % failures)
	quit(0 if failures == 0 else 1)

func _check_physics() -> void:
	var sandbox := Node2D.new()
	root.add_child(sandbox)
	var player: Player = PLAYER_SCENE.instantiate()
	sandbox.add_child(player)
	player.shot_timer.stop()
	for action: StringName in [&"left", &"right", &"up", &"down"]:
		var before := player.position
		Input.action_press(action)
		await create_timer(0.08).timeout
		Input.action_release(action)
		var movement := player.position - before
		var direction: Vector2 = {&"left": Vector2.LEFT, &"right": Vector2.RIGHT, &"up": Vector2.UP, &"down": Vector2.DOWN}[action]
		check(movement.dot(direction) > 0.0, "Input action moves player: " + action)
	player.position = Vector2(-100, -100)
	var enemy: Enemy = ENEMY_SCENE.instantiate()
	enemy.position = Vector2(60, 0)
	sandbox.add_child(enemy)
	var bullet: Bullet = BULLET_SCENE.instantiate()
	bullet.position = Vector2(0, 9)
	sandbox.add_child(bullet)
	await create_timer(0.25).timeout
	check(enemy.is_dead and not is_instance_valid(bullet), "Physics collision defeats enemy and removes bullet")
	await create_timer(0.65).timeout
	check(not is_instance_valid(enemy), "Death presentation cleans up enemy")
	var expiring: Bullet = BULLET_SCENE.instantiate()
	expiring.lifetime = 0.05
	sandbox.add_child(expiring)
	await create_timer(0.1).timeout
	check(not is_instance_valid(expiring), "Bullet expires without a hit")
	var attacker: Enemy = ENEMY_SCENE.instantiate()
	attacker.position = player.position
	sandbox.add_child(attacker)
	await create_timer(0.1).timeout
	check(player.is_dead, "Physics body collision kills player")
	sandbox.queue_free()
	await process_frame
