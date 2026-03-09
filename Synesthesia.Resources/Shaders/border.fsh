#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform vec4 topLeftColor;
uniform vec4 topRightColor;
uniform vec4 bottomLeftColor;
uniform vec4 bottomRightColor;
uniform vec2 size;
uniform int borderThickness;
uniform float cornerRadius;
uniform int inside;

out vec4 finalColor;

void main() {
    vec4 top = mix(topLeftColor, topRightColor, fragTexCoord.x);
    vec4 bottom = mix(bottomLeftColor, bottomRightColor, fragTexCoord.x);
    vec4 gradientColor = mix(top, bottom, fragTexCoord.y);

    float thick = float(borderThickness);
    vec2 p = (fragTexCoord - 0.5) * size;
    vec2 halfSize = size * 0.5;

    vec2 q = abs(p) - halfSize + cornerRadius;
    float dist = min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - cornerRadius;

    float smoothing = 1.0;
    float outerEdge = 1.0 - smoothstep(-smoothing, 0.0, dist);

    float innerEdge;
    if (inside == 1) {
        innerEdge = 1.0;
    } else {
        innerEdge = smoothstep(-thick - smoothing, -thick, dist);
    }

    float alpha = outerEdge * innerEdge;

    finalColor = vec4(gradientColor.rgb, gradientColor.a * alpha);
}