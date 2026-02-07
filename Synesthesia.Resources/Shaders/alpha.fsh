#version 330

// Input from Raylib's vertex shader
in vec2 fragTexCoord;
in vec4 fragColor;

// Your custom uniform
uniform sampler2D texture0;
uniform float alpha;

out vec4 finalColor;

void main() {
    vec4 texelColor = texture(texture0, fragTexCoord);

    vec4 outColor = texelColor * fragColor;
    outColor.a = alpha;
    finalColor = outColor;
}