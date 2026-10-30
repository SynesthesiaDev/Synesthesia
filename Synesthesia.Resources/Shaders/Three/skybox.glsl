#pragma kind Three

#pragma vertex
#version 330 core

// Keep all your layouts exactly the same so the engine doesn't complain!
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

// We only need to pass the direction to the fragment shader
out vec3 v_direction;

void main() {
    // Reconstruct the model matrix from instancing data, exactly like your default shader
    mat4 model = mat4(a_model_0, a_model_1, a_model_2, a_model_3);

    // The local position of the cube acts as our direction vector
    v_direction = a_position;

    // Calculate the standard position using your pre-calculated matrices
    vec4 pos = u_perspective * model * vec4(a_position, 1.0);

    // THE DEPTH TRICK: 
    // Swizzling .xyww forces Z to equal W, which after perspective division 
    // becomes 1.0 (the maximum depth). This draws it behind everything else.
    gl_Position = pos.xyww;
}

//-----------------------------------------------------------------------------------
#pragma fragment
#version 330 core

in vec3 v_direction;

// Reusing your default uniform names so the engine doesn't need new binding logic
uniform vec3 u_lightDirection;
uniform vec4 u_lightColor;
uniform float u_ambientLight;
uniform sampler2D u_texture;
uniform int u_useTexture;
uniform vec4 u_tint;

out vec4 FragColor;

const vec2 invAtan = vec2(0.1591, 0.3183);

void main() {
    vec3 dir = normalize(v_direction);
    dir.y = clamp(dir.y, -0.9999, 0.9999);
    
    vec2 uv = vec2(atan(dir.z, dir.x), asin(dir.y));
    
    uv *= invAtan;
    uv += 0.5;

    vec4 texColor;
    if (u_useTexture == 1) {
        vec2 dx = dFdx(uv);
        vec2 dy = dFdy(uv);
        texColor = textureGrad(u_texture, uv, dx, dy);
    } else {
        texColor = vec4(1.0, 0.0, 1.0, 1.0); // Magenta if false
    }


    
    FragColor = vec4(texColor.rgb * u_tint.rgb, 1.0);
}