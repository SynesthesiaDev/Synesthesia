#version 330 core
in vec2 v_texCoord;
in vec4 v_color;

out vec4 FragColor;

uniform sampler2D u_texture;
uniform int u_useTexture;

void main()
{
    vec4 texColor;

    if (u_useTexture == 1) {
        texColor = texture(u_texture, v_texCoord) * v_color;
    }
    else {
        texColor = v_color;
    }

    if (texColor.a <= 0.05) {
        discard;
    }

    FragColor = texColor;
}