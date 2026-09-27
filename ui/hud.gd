class_name HUD
extends CanvasLayer

@onready var score_label: Label = $Label
@onready var game_over_label: Label = $GameOverLabel

func set_score(value: int) -> void:
	score_label.text = "Score:" + str(value)

func show_game_over() -> void:
	game_over_label.show()
