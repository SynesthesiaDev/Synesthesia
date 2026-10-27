#version 330 core

layout (location = 0) in vec3 a_position;
layout (location = 1) in vec3 a_normal;
layout (location = 2) in vec3 a_tangent;
layout (location = 3) in vec4 a_color;
layout (location = 4) in vec2 a_texCoord;
layout (location = 5) in vec4 a_model_0;
layout (location = 6) in vec4 a_model_1;
layout (location = 7) in vec4 a_model_2;
layout (location = 8) in vec4 a_model_3;

uniform mat4 u_transform;
uniform mat4 u_perspective;

out vec3 v_position;
out vec3 v_normal;
out vec3 v_tangent;
out vec4 v_color;
out vec2 v_texCoord;
out mat4 v_model;

void main() {

    mat4 model = mat4(a_model_0, a_model_1, a_model_2, a_model_3);
    vec4 worldProjection = model * vec4(a_position, 1.0);
    
    gl_Position = u_perspective * model * vec4(a_position, 1.0);
    
    mat3 normalMatrix = transpose(inverse(mat3(model)));

    v_position = worldProjection.xyz;
    v_normal   = normalMatrix * a_normal;
    v_tangent  = normalMatrix * a_tangent;
    v_color    = a_color;
    v_texCoord = a_texCoord;
    v_model = model;
}