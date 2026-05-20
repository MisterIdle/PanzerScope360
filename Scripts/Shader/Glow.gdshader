shader_type canvas_item;

uniform float outline_width : hint_range(0.0, 20.0) = 5.0;
uniform vec4 outline_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

void fragment() {
    vec4 original_color = texture(screen_texture, SCREEN_UV);
    vec2 offset = SCREEN_PIXEL_SIZE * outline_width; 
    
    float alpha = original_color.a;
    
    alpha = max(alpha, texture(screen_texture, SCREEN_UV + vec2(offset.x, 0.0)).a);
    alpha = max(alpha, texture(screen_texture, SCREEN_UV + vec2(-offset.x, 0.0)).a);
    alpha = max(alpha, texture(screen_texture, SCREEN_UV + vec2(0.0, offset.y)).a);
    alpha = max(alpha, texture(screen_texture, SCREEN_UV + vec2(0.0, -offset.y)).a);
    
    float diag = 0.7071; 
    alpha = max(alpha, texture(screen_texture, SCREEN_UV + vec2(offset.x * diag, offset.y * diag)).a);
    alpha = max(alpha, texture(screen_texture, SCREEN_UV + vec2(-offset.x * diag, offset.y * diag)).a);
    alpha = max(alpha, texture(screen_texture, SCREEN_UV + vec2(offset.x * diag, -offset.y * diag)).a);
    alpha = max(alpha, texture(screen_texture, SCREEN_UV + vec2(-offset.x * diag, -offset.y * diag)).a);

    vec4 final_outline = outline_color;
    final_outline.a = alpha * outline_color.a * COLOR.a;
    
    COLOR = mix(final_outline, original_color * COLOR, original_color.a);
}