#version 330 core

in vec2 v_texCoord;
in vec4 v_color;
in vec2 v_size;
in float v_radius;
in vec2 v_localUV;

uniform sampler2D u_texture;
uniform int u_useTexture;

out vec4 FragColor;

void main() {
    vec2 p = (v_localUV - 0.5) * v_size;

    vec2 b = (v_size * 0.5) - vec2(v_radius);
    vec2 q = abs(p) - b;
    float dist = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - v_radius;

    float edgeSoftness = fwidth(dist);
    float alpha = 1.0 - smoothstep(-edgeSoftness, edgeSoftness, dist);

    vec4 finalColor;
    if (u_useTexture == 1) {
        finalColor = texture(u_texture, v_texCoord) * v_color;
    } else {
        finalColor = v_color;
    }

    FragColor = vec4(finalColor.rgb, finalColor.a * alpha);
}