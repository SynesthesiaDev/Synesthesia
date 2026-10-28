#pragma kind Three

#pragma vertex
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

//-----------------------------------------------------------------------------------
#pragma fragment
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
uniform float u_texture_scale;

out vec4 FragColor;

void main() {
    vec3 normal = normalize(v_normal);
    float diffuse = max(dot(normal, -normalize(u_lightDirection)), 0.0);
    vec3 light = vec3(u_ambientLight) + u_lightColor.rgb * diffuse;

    vec4 texColor = vec4(1.0);

    if (u_useTexture == 1) {
        vec3 blend = abs(normal);
        blend = smoothstep(0.0, 1.0, blend);
        blend /= (blend.x + blend.y + blend.z);

        vec2 uvX = v_position.zy * u_texture_scale;
        vec2 uvY = v_position.xz * u_texture_scale;
        vec2 uvZ = v_position.xy * u_texture_scale;

        vec4 texX = texture(u_texture, uvX);
        vec4 texY = texture(u_texture, uvY);
        vec4 texZ = texture(u_texture, uvZ);

        texColor = texX * blend.x + texY * blend.y + texZ * blend.z;
    }

    FragColor = vec4(light * v_color.rgb * texColor.rgb * u_tint.rgb, v_color.a * texColor.a);
}