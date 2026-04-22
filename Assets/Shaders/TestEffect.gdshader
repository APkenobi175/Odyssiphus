shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
		vec2 dist = vec2(0.5) - UV;
		vec2 pinch = normalize(dist);
		COLOR.bar = texture(screen_texture, SCREEN_UV + pinch * smoothstep(0.5, 0.0, length(dist)) * 0.25).gbr;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
