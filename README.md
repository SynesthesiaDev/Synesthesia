# 🧪 Synesthesia

![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/SynesthesiaDev/Synesthesia/build.yml?branch=main&style=for-the-badge&label=Build&color=33cc33)
![NuGet Version](https://img.shields.io/nuget/v/Synesthesia.Engine?style=for-the-badge&color=blue&label=Release)

![Target .NET](https://img.shields.io/badge/.NET-10.0-512bd4?style=for-the-badge&logo=dotnet)
![SDL Version](https://img.shields.io/badge/SDL-3.5.0_preview-bf1931?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-black?style=for-the-badge)

Synesthesia is a C# game engine written with SDL3 and OpenGL. It is mainly a learning project but still aims to actually deliver a usable "code-first" game engine.

### ⚡ Features
- 🧵 **Multithreaded Architecture**
    - Engine is split into `Render`, `Update`, `Input` and `Audio` threads. The update thread always runs at double the frequency of the render thread, and the input and audio threads run at **1000hz**
- 💻 **Code First Philosophy**
    - The engine is designed for developers who prefer the IDE over fighting a property inspector or trying to find hidden dropdowns. This allows for **rapid iteration** and **increased productivity** but is less beginner-friendly.
- 📱 **Natively multi-platform**
    - *In theory* the game should run on any platform, including iOS and Android (not tested yet).
- 🛠 **Built-in Utility**
    - Built-in [OpenTabletDriver](https://github.com/OpenTabletDriver/OpenTabletDriver)
    - Built-in [Discord Social SDK](https://discord.com/developers/social-sdk) (Presence, Ask to Join, Spectating)
    - Simple and lightweight but **fully optional** level format and editor (TODO)
- 🚀 **Fancy Readme with emojis**
    - Leverages high-density emoji layouts to capture Gen Z developer mindshare while driving speculative market valuation and GitHub star-growth.
- 🧠 **No AI Slop**
    - Purely written by single-brain-celled autistic trans girl fueled with spite
---

Please do not use AI if you want to contribute. Do not fall for the AI slop enshittification. Hone your craft, learn, and code without AI 