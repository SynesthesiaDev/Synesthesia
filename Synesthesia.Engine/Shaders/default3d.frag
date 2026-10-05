#version 330 core

in vec3 v_position;
in vec3 v_normal;
in vec3 v_tangent;
in vec4 v_color;
in vec2 v_texCoord;

uniform vec3 u_lightDirection;
uniform vec3 u_lightColor;
uniform float u_ambientLight;

out vec4 FragColor;

void main() {
    vec3 normal = normalize(v_normal);
    float diffuse = max(dot(normal, normalize(u_lightDirection)), 0.0);
    vec3 color = u_lightColor * diff * u_ambientLight; 
    FragColor = color;
}