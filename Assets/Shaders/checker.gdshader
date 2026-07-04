shader_type canvas_item;

uniform float cell_size = 64.0;
uniform vec4 color_a : source_color = vec4(0.18, 0.18, 0.20, 1.0);
uniform vec4 color_b : source_color = vec4(0.24, 0.24, 0.27, 1.0);

varying vec2 world_pos;

void vertex() {
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
	vec2 cell = floor(world_pos / cell_size);
	COLOR = mix(color_a, color_b, mod(cell.x + cell.y, 2.0));
}
