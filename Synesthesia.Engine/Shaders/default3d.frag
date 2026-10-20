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
uniform int u_tint;

out vec4 FragColor;

void main() {
    vec3 normal = normalize(v_normal);
    float diffuse = max(dot(normal, -normalize(u_lightDirection)), 0.0);
    vec3 light = vec3(u_ambientLight) + u_lightColor.rgb * diffuse;

    vec4 texColor = (u_useTexture == 1) ? texture(u_texture, v_texCoord) : vec4(1.0);
    FragColor = vec4(light * v_color.rgb * texColor.rgb, v_color.a * texColor.a);
}