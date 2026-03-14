// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine;

public static class Defaults
{
    public const int RENDER_THREAD_HZ = 240;
    public const int UPDATE_THREAD_HZ = 480;
    public const int AUDIO_THREAD_HZ = 1000;
    public const int INPUT_THREAD_HZ = 1000;

    public const string DEFAULT_VERTEX_SHADER = """

                                                        #version 330 core
                                                        layout(location = 0) in vec3 a_position;
                                                        layout(location = 1) in vec4 a_color;

                                                        uniform mat4 u_transform;

                                                        out vec4 v_color;

                                                        void main()
                                                        {
                                                            gl_Position = u_transform * vec4(a_position, 1.0);
                                                            v_color = a_color;
                                                        }

                                                """;

    public const string DEFAULT_FRAGMENT_SHADER = """

                                                          #version 330 core
                                                          in vec4 v_color;
                                                          out vec4 FragColor;

                                                          void main()
                                                          {
                                                              FragColor = v_color;
                                                          }

                                                  """;
}
