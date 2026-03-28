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
        float smoothing = 1.5 / v_size.y;
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

        if(v_borderThickness > 0.5) {
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