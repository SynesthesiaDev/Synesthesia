#version 330 core

in vec2 v_texCoord;
in vec4 v_color;
in vec2 v_size;
in float v_radius; // Shape mode: Corner Radius | Text mode: SDF Thickness
in vec2 v_localUV;
in float v_mode;

uniform sampler2D u_texture;
uniform int u_useTexture;

out vec4 FragColor;

const float VERTEX_MODE_SHAPE = 0.0;
const float VERTEX_MODE_TEXT = 1.0;

void main() {
    float alpha = 1.0;
    vec4 texColor = vec4(1.0);

    if (v_mode == VERTEX_MODE_TEXT) {
        float threshold = 0.5 - (v_radius - 0.1);
        float dist = texture(u_texture, v_texCoord).a - 0.1;
        float smoothing = 1.9 / v_size.y;
        float localAlpha = smoothstep(threshold - smoothing, threshold + smoothing, dist);
        FragColor = vec4(v_color.rgb, v_color.a * localAlpha);
        return;
        
    } else if (v_mode == VERTEX_MODE_SHAPE) {
        vec2 p = (v_localUV - 0.5) * v_size;
        vec2 b = (v_size * 0.5) - vec2(v_radius);
        vec2 q = abs(p) - b;
        float dist = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - v_radius;

        float edgeSoftness = fwidth(dist);
        alpha = 1.0 - smoothstep(-edgeSoftness, edgeSoftness, dist);

        if (u_useTexture == 1) {
            texColor = texture(u_texture, v_texCoord);
        }

        vec4 finalColor = texColor * v_color;
        FragColor = vec4(finalColor.rgb, finalColor.a * alpha);
    }
}