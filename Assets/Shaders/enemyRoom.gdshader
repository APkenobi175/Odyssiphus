shader_type canvas_item;

uniform float depth : hint_range(0.0, 1.0) = 0.0;

const vec4 beach = vec4(0.94, 0.87, 0.65, 1.0);    // sandy yellow
const vec4 mountain = vec4(0.55, 0.50, 0.45, 1.0); // rocky grey
const vec4 volcano = vec4(0.60, 0.25, 0.10, 1.0);  // dark red
const vec4 ocean = vec4(0.10, 0.30, 0.60, 1.0);    // deep blue

void vertex() {
	// Called for every vertex the material is visible on.
	
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec4 tex = texture(TEXTURE, UV);
	
	vec4 biomeColor;
	if(depth < 0.33){
		biomeColor = mix(beach, mountain, depth / 0.33);
	}else if (depth < 0.66){
		biomeColor = mix(mountain, volcano, (depth - 0.33) / 0.33);
		
	}else{
		biomeColor = mix(volcano, ocean, (depth - 0.66) / 0.34);
	}
	
	COLOR = tex * biomeColor;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
