#version 330 core

layout(location = 0) in vec3 a_position;
layout(location = 1) in vec3 a_normal;
layout(location = 2) in vec3 a_tangent;
layout(location = 3) in vec4 a_color;
layout(location = 4) in vec2 a_texCoord;

uniform mat4 u_transform;

out vec3 v_position;
out vec3 v_normal;
out vec3 v_tangent;
out vec4 v_color;
out vec2 v_texCoord;

void main() {
    gl_position = u_transform * vec4(a_position, 1.0);
    v_position = a_position;
    v_normal = a_normal;
    v_tangent = a_tangent;
    v_color = a_color;
    v_texCoord = a_texCoord;
}