// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Platform.Host;
using Synesthesia.Engine.Resources.Stores;

namespace Synesthesia.Engine;

public class GameBuilder
{
    private string windowName = $"{Assembly.GetCallingAssembly().GetName()}";
    private Action<FallbackStoreBuilder<Texture>>? textureResourceStoreInitializer;
    private Action<FallbackStoreBuilder<TextureAtlas>>? textureAtlasResourceStoreInitializer;
    private Action<FallbackStoreBuilder<Font>>? fontResourceStoreInitializer;
    private IWindowHost windowHost = new SDL3WindowHost();

    public GameBuilder AddTextureSources(Action<FallbackStoreBuilder<Texture>> action)
    {
        textureResourceStoreInitializer = action;
        return this;
    }

    public GameBuilder AddTextureAtlasSources(Action<FallbackStoreBuilder<TextureAtlas>> action)
    {
        textureAtlasResourceStoreInitializer = action;
        return this;
    }

    public GameBuilder AddFontSources(Action<FallbackStoreBuilder<Font>> action)
    {
        fontResourceStoreInitializer = action;
        return this;
    }

    public GameBuilder SetWindowHost(IWindowHost host)
    {
        windowHost = host;
        return this;
    }

    public GameBuilder SetWindowName(string name)
    {
        windowName = name;
        return this;
    }

    public Game Build()
    {
        return new Game(windowHost, textureResourceStoreInitializer, textureAtlasResourceStoreInitializer, fontResourceStoreInitializer, windowName);
    }
}
