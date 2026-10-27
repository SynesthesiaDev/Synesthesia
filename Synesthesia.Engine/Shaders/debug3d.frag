#version 330 core

in vec3 v_position;
in vec3 v_normal;
in vec3 v_tangent;
in vec4 v_color;
in vec2 v_texCoord;

uniform vec3 u_lightDirection;
uniform vec4 u_lightColor;
uniform float u_ambientLight;
uniform sampler2D u_texture;
uniform int u_useTexture;
uniform vec4 u_tint;

uniform float u_scale = 2.0;
uniform vec3 u_colorA = vec3(0.9);
uniform vec3 u_colorB = vec3(0.2);

out vec4 FragColor;

void main() {
    vec2 scaledUv = v_texCoord * u_scale;
    vec2 grid = floor(scaledUv);
    vec2 f = fract(scaledUv);

    // 1. Source-style checkerboard (Alternating tiles)
    float checker = mod(grid.x + grid.y, 2.0);

    // 2. Anti-aliased grid lines & border detection
    vec2 fw = fwidth(scaledUv);
    float border = 1.0 - smoothstep(0.0, 0.05, min(f.x, f.y) * (1.0 - step(0.95, f.x)) * (1.0 - step(0.95, f.y)));

    // 3. Source dev texture "cut corner" mark (top-left 1/8th notch)
    float cornerMark = step(f.x, 0.15) * step(f.y, 0.15);

    // 4. Directional bevel lighting to give tiles depth
    vec2 centerDir = vec2(0.5) - f;
    float bevel = clamp((centerDir.x + centerDir.y) * 0.5, -1.0, 1.0);

    // Base color assignment
    vec3 tileColor = mix(u_colorA, u_colorB, checker);

    // Apply subtle bevel shading to simulate raised tiles
    tileColor *= (0.9 + 0.15 * bevel);

    // Blend grid lines (white) and corner marks
    vec3 lineColor = vec3(1.0);
    vec3 finalColor = mix(tileColor, lineColor, border);
    finalColor = mix(finalColor, lineColor, cornerMark);

    // Preserve your lighting calculation
    vec3 normal = normalize(v_normal);
    float diffuse = max(dot(normal, -normalize(u_lightDirection)), 0.0);
    vec3 light = vec3(u_ambientLight) + u_lightColor.rgb * diffuse;

    vec4 texColor = (u_useTexture == 1) ? texture(u_texture, v_texCoord) : vec4(1.0);
    FragColor = vec4(light * v_color.rgb * finalColor * u_tint.rgb, v_color.a * texColor.a);
}