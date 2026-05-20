shader_type canvas_item;

uniform float intensity : hint_range(0.0, 5.0) = 1.0;
uniform float radius : hint_range(0.0, 1.0) = 0.5;

void fragment() {
    float dist = distance(UV, vec2(0.5));
    
    float mask = 1.0 - smoothstep(0.0, radius, dist);
    
    mask *= intensity;
	
    COLOR = vec4(0.0, 0.0, 0.0, mask);
}