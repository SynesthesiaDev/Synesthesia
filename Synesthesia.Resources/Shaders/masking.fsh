#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colorDiffuse;

uniform vec2 maskPosition;
uniform vec2 maskSize;
uniform float cornerRadius;
uniform int maskingEnabled;

out vec4 finalColor;

float roundedBoxSDF(vec2 centerPos, vec2 size, float radius) {
    return length(max(abs(centerPos) - size + radius, 0.0)) - radius;
}

void main() {
    vec4 texelColor = texture(texture0, fragTexCoord);
    finalColor = texelColor * colorDiffuse * fragColor;
    
    if(maskingEnabled == 1) {
        vec2 fragPos = gl_FragCoord.xy;
        
        vec2 maskCenter = maskPosition + maskSize * 0.5;
        vec2 halfSize = maskSize * 0.5;
        vec2 relativePos = fragPos - maskCenter;
        
        float distance = roundedBoxSDF(relativePos, halfSize, cornerRadius);
        
        if(distance > 0.0) {
            discard;
        }
        
        float edgeSoftness = 1.0;
        float alpha = 1.0 - smoothstep(-edgeSoftness, edgeSoftness, distance);
        finalColor.a *= alpha;
    }
}