//#version 330
//in vec2 v_texCoord;
//in vec4 v_color;
//
//out vec4 FragColor;
//
//uniform sampler2D u_texture;
//uniform int u_useTexture;
//
//void main()
//{
//    if (u_useTexture == 1) {
//        FragColor = texture(u_texture, v_texCoord) * v_color;
//    }
//    else {
//        FragColor = v_color;
//    }
//}
#version 330 core
out vec4 FragColor;
void main() {
    FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}