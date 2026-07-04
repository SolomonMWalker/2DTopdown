shader_type canvas_item;

// Darkens the edges of the rect: transparent at center, fading to black at the corners.
uniform float strength : hint_range(0.0, 1.0) = 0.45;
uniform float inner : hint_range(0.0, 1.0) = 0.55; // where the darkening starts
uniform float outer : hint_range(0.0, 1.5) = 0.95; // where it reaches full strength

void fragment() {
	float d = distance(UV, vec2(0.5)) * 1.41421356; // 0 at center, ~1 at corners
	float v = smoothstep(inner, outer, d) * strength;
	COLOR = vec4(0.0, 0.0, 0.0, v);
}
