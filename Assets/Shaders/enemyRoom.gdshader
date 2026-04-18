shader_type canvas_item;

uniform float depth : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    
    float lum = dot(tex.rgb, vec3(0.299, 0.587, 0.114));
    
    vec3 sand    = vec3(0.90, 0.80, 0.55); // warm golden sand
    vec3 grey    = vec3(0.72, 0.72, 0.70); // neutral grey
    vec3 volcano = vec3(0.75, 0.30, 0.20); // fiery red
    vec3 ocean   = vec3(0.20, 0.75, 0.70); // turquoise

    vec3 biomeColor;
    if(depth < 0.33){
        biomeColor = mix(sand, grey, depth / 0.33);
    } else if(depth < 0.66){
        biomeColor = mix(grey, volcano, (depth - 0.33) / 0.33);
    } else {
        biomeColor = mix(volcano, ocean, (depth - 0.66) / 0.34);
    }
    
    vec3 tinted = mix(tex.rgb, lum * biomeColor * 2.0, 0.6);
    COLOR = vec4(tinted, tex.a);
}