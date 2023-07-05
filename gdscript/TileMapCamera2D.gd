extends Camera2D
class_name TileMapCamera2D

@export var speed: float = 16.0
@export var initial_zoom: Vector2 = Vector2(0.275, 0.275)
@export var initial_position: Vector2i = Vector2(490, 256)

@export var zoom_inc: float = 0.075

var following_object: Node2D

func _ready():
	make_current()
	position = initial_position
	zoom = initial_zoom

func _input(_event):	
	make_zoom()
	move_camera()
	
func make_zoom() -> void:
	# Mejorar para suavizar el movimiento
	var zoom_sign: int = 0
	zoom_sign = int(Input.is_action_pressed("tilemap_camera_zoom_in")) - int(Input.is_action_pressed("tilemap_camera_zoom_out"))
	if zoom_sign != 0:	set_zoom(Vector2(max(zoom.x + zoom_inc*zoom_sign, 0.1), max(zoom.y + zoom_inc*zoom_sign, 0.1)))	
	
func move_camera() -> void:
	# Implements camera movement by using ui keys. 'camera_' actions are custom actions (Project settings -> Input map)

	if following_object:
		return
	var input_x: int = int(Input.is_action_pressed("tilemap_camera_right")) - int(Input.is_action_pressed("tilemap_camera_left"))	
	var input_y: int = int(Input.is_action_pressed("tilemap_camera_down")) - int(Input.is_action_pressed("tilemap_camera_up"))	
	
	# Update camera position
	position.x += input_x * speed
	position.y += input_y * speed
	
