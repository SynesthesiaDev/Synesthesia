#version 330 core

layout (location = 0) in vec3 a_position;
layout (location = 1) in vec3 a_normal;
layout (location = 2) in vec3 a_tangent;
layout (location = 3) in vec4 a_color;
layout (location = 4) in vec2 a_texCoord;

uniform mat4 u_model;
uniform mat4 u_transform;
uniform mat4 u_perspective;

out vec3 v_position;
out vec3 v_normal;
out vec3 v_tangent;
out vec4 v_color;
out vec2 v_texCoord;

void main() {
    vec4 worldProjection = u_model * vec4(a_position, 1.0);
    gl_Position = u_perspective * worldProjection;

    mat3 normalMatrix = transpose(inverse(mat3(u_model)));

    v_position = worldProjection.xyz;
    v_normal   = normalMatrix * a_normal;
    v_tangent  = normalMatrix * a_tangent;
    v_color    = a_color;
    v_texCoord = a_texCoord;
}