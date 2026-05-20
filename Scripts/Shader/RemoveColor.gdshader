shader_type canvas_item;

uniform bool is_loser = false;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    float gray = dot(tex_color.rgb, vec3(0.299, 0.587, 0.114));
    COLOR = mix(tex_color, vec4(gray, gray, gray, tex_color.a), float(is_loser));
}