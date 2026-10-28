#pragma kind Two

#pragma vertex
#version 330 core

layout (location = 0) in vec2 a_position;
layout (location = 1) in vec2 a_texCoord;
layout (location = 2) in vec2 a_size;
layout (location = 3) in vec4 a_color;
layout (location = 4) in float a_alpha;
layout (location = 5) in float a_radius;
layout (location = 6) in vec2 a_localUV;
layout (location = 7) in float a_mode;
layout (location = 8) in float a_borderThickness;
layout (location = 9) in float a_borderHasSingleColor;
layout (location = 10) in mat4 a_borderColor;

uniform mat4 u_transform;

out vec2 v_texCoord;
out vec4 v_color;
out vec2 v_size;
out float v_radius;
out vec2 v_localUV;
out float v_mode;
out float v_borderThickness;
out float v_borderHasSingleColor;
out mat4 v_borderColor;
out float v_alpha;

void main() {
    v_texCoord = a_texCoord;
    v_color = a_color;
    v_size = a_size;
    v_radius = a_radius;
    v_localUV = a_localUV;
    v_mode = a_mode;
    v_borderThickness = a_borderThickness;
    v_borderColor = a_borderColor;
    v_borderHasSingleColor = a_borderHasSingleColor;
    v_alpha = a_alpha;

    gl_Position = u_transform * vec4(a_position, 0.0, 1.0);
}

//-----------------------------------------------------------------------------------

#pragma fragment
#version 330 core

in vec2 v_texCoord;
in vec4 v_color;
in float v_alpha;
in vec2 v_size;
in float v_radius; // Shape mode: Corner Radius | Text mode: SDF Thickness
in vec2 v_localUV;
in float v_mode;
in float v_borderThickness; // 0f means no border
in float v_borderHasSingleColor;
in mat4 v_borderColor;

uniform sampler2D u_texture;
uniform int u_useTexture;

out vec4 FragColor;

const float VERTEX_MODE_SHAPE = 0.0;
const float VERTEX_MODE_TEXT = 1.0;

void main() {
    float alpha = 1.0;

    if (v_mode == VERTEX_MODE_TEXT) {
        float threshold = 0.5 - (v_radius - 0.1);
        float dist = texture(u_texture, v_texCoord).a - 0.1;
        float smoothing = 2.0 / v_size.y;
        float localAlpha = smoothstep(threshold - smoothing, threshold + smoothing, dist);
        FragColor = vec4(v_color.rgb, (v_color.a * localAlpha) * v_alpha);
        return;

    } else if (v_mode == VERTEX_MODE_SHAPE) {
        vec2 p = (v_localUV - 0.5) * v_size;
        vec2 b = (v_size * 0.5) - vec2(v_radius);
        vec2 q = abs(p) - b;
        float dist = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - v_radius;

        float edgeSoftness = fwidth(dist);
        float outerAlpha = 1.0 - smoothstep(-edgeSoftness, edgeSoftness, dist);

        vec4 texColor = (u_useTexture == 1) ? texture(u_texture, v_texCoord) : vec4(1.0);
        vec4 fillColor = texColor * v_color;

        alpha = 1.0 - smoothstep(-edgeSoftness, edgeSoftness, dist);

        if (v_borderThickness > 0.5) {
            vec4 finalBorderColor;

            if (v_borderHasSingleColor > 0.5) {
                finalBorderColor = v_borderColor[0];
            } else {
                vec4 colorTop = mix(v_borderColor[0], v_borderColor[1], v_localUV.x);
                vec4 colorBottom = mix(v_borderColor[2], v_borderColor[3], v_localUV.x);
                finalBorderColor = mix(colorTop, colorBottom, v_localUV.y);
            }

            float fillDist = dist + v_borderThickness;
            float fillAlpha = 1.0 - smoothstep(-edgeSoftness, edgeSoftness, fillDist);

            vec4 combinedColor = mix(finalBorderColor, fillColor, fillAlpha);
            FragColor = vec4(combinedColor.rgb, (combinedColor.a * outerAlpha) * v_alpha);
        } else {
            FragColor = vec4(fillColor.rgb, (fillColor.a * outerAlpha) * v_alpha);
        }
    }
}