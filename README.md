# Synesthesia Engine

![Language](https://img.shields.io/badge/language-C%23-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)

Synesthesia is a game engine written on top of Raylib (in the future, may discontinue raylib to support multi-renderer architecture)

The architecture and workflow is inspired by the [osu!framework](https://github.com/ppy/osu-framework/)

Capabilities:
- Multithreaded architecture
  - Render, Update, Input and Audio are on seperate threads. Input and Audio threads run at 1000 fps, Update thread runs at double fps of Render thread
- Improved Audio
  - Synesthesia utilizes the [BASS](https://www.un4seen.com/bass.html) library for audio instead of built-in raylib audio which allows for better control, lower latency and [Wasapi](https://learn.microsoft.com/en-us/windows/win32/coreaudio/wasapi) support
- Powerful flow layout system for 2d which allows for making complex UIs very easily

3D is work in progress (the whole engine is)