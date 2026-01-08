#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;
uniform float renderSize;
uniform float alpha;

out vec4 finalColor;

void main()
{
    float distanceFromOutline = texture(texture0, fragTexCoord).a - 0.1;
    float smoothing = 7.0 / renderSize;
    float localAlpha = smoothstep(smoothing - 0.5, smoothing + 0.5, distanceFromOutline);

    float finalAlpha = fragColor.a * localAlpha * alpha;
    finalColor = vec4(fragColor.rgb, finalAlpha);
}
