#version 330 core

layout(location = 0) in vec2 a_position;
layout(location = 1) in vec2 a_texCoord;
layout(location = 2) in vec2 a_size;
layout(location = 3) in vec4 a_color;
layout(location = 4) in float a_radius;
layout(location = 5) in vec2 a_localUV;

uniform mat4 u_transform;

out vec2 v_texCoord;
out vec4 v_color;
out vec2 v_size;
out float v_radius;
out vec2 v_localUV;

void main() {
    v_texCoord = a_texCoord;
    v_color = a_color;
    v_size = a_size;
    v_radius = a_radius;
    v_localUV = a_localUV;
    
    gl_Position = u_transform * vec4(a_position, 0.0, 1.0);
}