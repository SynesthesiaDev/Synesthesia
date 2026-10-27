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

uniform float u_scale = 10.0;
uniform vec3 u_colorA = vec3(0.9);
uniform vec3 u_colorB = vec3(0.2);

out vec4 FragColor;

void main() {
    vec2 scaledUv = v_texCoord * u_scale;
    
    vec2 grid = floor(scaledUv);
    float checker = mod(grid.x + grid.y, 2.0);

    vec2 fw = fwidth(scaledUv);
    float blend = smoothstep(0.5 - fw.x, 0.5 + fw.x, fract(scaledUv.x)) *
    smoothstep(0.5 - fw.y, 0.5 + fw.y, fract(scaledUv.y));
    
    checker = mix(checker, 0.5, blend);
    vec3 finalColor = mix(u_colorA, u_colorB, checker);
    
    vec3 normal = normalize(v_normal);
    float diffuse = max(dot(normal, -normalize(u_lightDirection)), 0.0);
    vec3 light = vec3(u_ambientLight) + u_lightColor.rgb * diffuse;

    vec4 texColor = (u_useTexture == 1) ? texture(u_texture, v_texCoord) : vec4(1.0);
    FragColor = vec4(light * v_color.rgb * finalColor.rgb * u_tint.rgb, v_color.a * texColor.a);
}