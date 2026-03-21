#version 330 core

in vec2 v_texCoord;
in vec4 v_color;

uniform sampler2D u_texture;
uniform int u_useTexture;

out vec4 FragColor;

void main() {
    if (u_useTexture == 1) {
        FragColor = texture(u_texture, v_texCoord) * v_color;
    } else {
        FragColor = v_color;
    }
}