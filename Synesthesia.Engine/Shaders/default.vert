//#version 330
//layout (location = 0) in vec3 a_position;
//layout (location = 1) in vec2 a_texCoord;
//layout (location = 2) in vec4 a_color;
//
//uniform mat4 u_transform;
//
//out vec2 v_texCoord;
//out vec4 v_color;
//
//void main()
//{
//    v_texCoord = a_texCoord;
//    v_color = a_color;
//    gl_Position = u_transform * vec4(a_position, 1.0);
//}
#version 330 core

layout (location = 0) in vec3 aPos;
uniform mat4 u_transform;
void main() {
    gl_Position = u_transform * vec4(aPos, 1.0);
}