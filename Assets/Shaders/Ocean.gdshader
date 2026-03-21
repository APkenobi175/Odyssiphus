shader_type canvas_item;

void fragment() {
    vec2 uv = UV;
    
    float horizon = 0.85; // where sky meets ocean
    
    // Sky
    vec3 sky_top = vec3(0.02, 0.04, 0.12);
    vec3 sky_horizon = vec3(0.05, 0.10, 0.22);
    vec3 sky = mix(sky_top, sky_horizon, uv.y / horizon);
    
    // Stars
    vec2 star_uv = uv * 200.0;
    vec2 star_grid = fract(star_uv) - 0.5;
    vec2 star_id = floor(star_uv);
    float rand = fract(sin(dot(star_id, vec2(127.1, 311.7))) * 43758.5);
    float star = step(0.97, rand) * smoothstep(0.25, 0.0, length(star_grid));
    sky = mix(sky, vec3(0.8, 0.75, 0.5), star * step(uv.y, horizon));
    
    // Ocean
    vec3 ocean_surface = vec3(0.03, 0.08, 0.22);
    vec3 ocean_deep = vec3(0.01, 0.02, 0.08);
    vec3 ocean = mix(ocean_surface, ocean_deep, (uv.y - horizon) / (1.0 - horizon));
    
    // Waves
    float wave1 = sin(uv.x * 15.0 + TIME * 0.8) * 0.012;
    float wave2 = sin(uv.x * 25.0 - TIME * 1.2) * 0.006;
    float wave3 = sin(uv.x * 8.0 + TIME * 0.4) * 0.018;
    float waves = wave1 + wave2 + wave3;
    
    // Shimmer on wave crests
    float shimmer = smoothstep(0.01, 0.025, waves);
    ocean = mix(ocean, vec3(0.3, 0.5, 0.8), shimmer * 0.4);
    
    // Blend sky and ocean at horizon
    float blend = smoothstep(horizon - 0.02, horizon + 0.02, uv.y + waves);
    vec3 color = mix(sky, ocean, blend);
    
    COLOR = vec4(color, 1.0);
}