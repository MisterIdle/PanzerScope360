shader_type canvas_item;

uniform float edge_damage = 0.03;
uniform float edge_noise_scale = 8.0;

float random(vec2 uv) {
    return fract(sin(dot(uv.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

float noise(vec2 uv) {
    vec2 i = floor(uv);
    vec2 f = fract(uv);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(random(i), random(i + vec2(1.0, 0.0)), u.x),
               mix(random(i + vec2(0.0, 1.0)), random(i + vec2(1.0, 1.0)), u.x), u.y);
}

void fragment() {
    float noise_left = noise(vec2(UV.y * edge_noise_scale, 0.0));
    float noise_right = noise(vec2(UV.y * edge_noise_scale + 12.34, 0.0));
    float noise_top = noise(vec2(UV.x * edge_noise_scale + 45.67, 0.0));
    float noise_bottom = noise(vec2(UV.x * edge_noise_scale + 89.12, 0.0));

    float left = noise_left * edge_damage;
    float right = 1.0 - (noise_right * edge_damage);
    float top = noise_top * edge_damage;
    float bottom = 1.0 - (noise_bottom * edge_damage);

    float alpha_mask = step(left, UV.x) * step(UV.x, right) * step(top, UV.y) * step(UV.y, bottom);

    vec4 tex_color = texture(TEXTURE, UV);
    COLOR = tex_color * COLOR;
    COLOR.a *= alpha_mask;
}