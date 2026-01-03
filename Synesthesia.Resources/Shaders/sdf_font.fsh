#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;
uniform float renderSize;

out vec4 finalColor;

void main()
{
    float distanceFromOutline = texture(texture0, fragTexCoord).a - 0.25;
    float distanceChangePerFragment = length(vec2(dFdx(distanceFromOutline), dFdy(distanceFromOutline)));
    float smoothing = 6.0 / renderSize;
    float alpha = smoothstep(smoothing - 0.5, smoothing + 0.5, distanceFromOutline);

    finalColor = vec4(fragColor.rgb, fragColor.a*alpha);
}
