# Learning Resources

The resources and knowledge I used when I was making this engine. This exists for both myself (dementia programming!!!) and others who want to learn or contribute

### Platform, Windowing etc.

I wrote this about year ago now from when im writing this document but I used the [osu!framework](https://github.com/ppy/osu-framework/) as a reference. Specific files:
- [GameHost.cs](https://github.com/ppy/osu-framework/blob/master/osu.Framework/Platform/GameHost.cs)
- [SDL3Window.cs](https://github.com/ppy/osu-framework/blob/master/osu.Framework/Platform/SDL3/SDL3Window.cs)
- [SDLGameHost.cs](https://github.com/ppy/osu-framework/blob/master/osu.Framework/Platform/SDLGameHost.cs)

### Graphics & Rendering
- I pretty much figured this out myself. I started small with very very simple renderer class with `BeginDrawing()` and `EndDrawing()` and everything was manual.
This class eventually grew with demands and then was split into `GraphicsDevice`, `Renderer2D` and `Renderer3D` when I was planning on adding 3D because it would be all mess in a single class.
- Vertex batching was yet again primarely learned from osu!framework
  - [TexturedVertex2D](https://github.com/ppy/osu-framework/blob/3d297e811caf3f1b1318359137cbefbace8da5b7/osu.Framework/Graphics/Rendering/Vertices/TexturedVertex2D.cs)
- I did initially want to use [Veldrid](https://github.com/veldrid/veldrid) just like osu!framework as well but I realised I got too deep in, cause I did not really understand graphics programming in the first place. My advice is just start very simple, when you understand the concepts you can move up a layer. OpenGL might be legacy these days but it still runs fairy well and is supported so I went with that
- For shaders I pretty much used [The Book of Shaders](https://easyguet.ch/temp/thebookofshaders.pdf), bunch of obscure stack overflow answers and [Godot's built-in shaders](https://github.com/search?q=repo%3Agodotengine%2Fgodot++language%3AGLSL&type=code). Honestly I still don't think I understand shaders super well but .. its functional and it works so thats the important part!

### Physics and Collision
- Honestly... I vomited something into the IDE and it kinda worked so idk. Its not good I need to rewrite it and actually look up how physics properly work

### Clocks and Timing
- Yet again, big inspiration was osu!framework, especially with [NativeSleep](https://github.com/ppy/osu-framework/blob/3d297e811caf3f1b1318359137cbefbace8da5b7/osu.Framework/Platform/Windows/Native/WindowsNativeSleep.cs) cause wow `Thread.Sleep` is really really inaccurate

### Resources and Asset pipeline
This used to be inspired by osu!framework (are you surprised?!) but over time it evolved into my own thing. The main phylosophy is that for example if we look at `TextureResourceStore`:

```csharp
TextureResourceStore = new ResourceStoreBuilder<Texture>()
    .AddLoaders(new Dictionary<string, Func<Stream, string, Texture>>(StringComparer.Ordinal)
    {
        { "png", (stream, name) => ResourceLoaders.LoadTexture(stream, name) },
        { "bmp", (stream, name) => ResourceLoaders.LoadTexture(stream, name) },
        { "jpg", (stream, name) => ResourceLoaders.LoadTexture(stream, name) },
    })
    .AddFallback(fallback =>
    {
        fallback.AddFileSystemStore("Assets/Textures/");
        fallback.AddAssemblyStream(AssemblyInfo.ResourceAssembly);
        textureResourceStoreInitializer?.Invoke(fallback);
    })
    .MakeCached()
    .MakeAsync()
    .MakeDeferred()
    .Build();
```

We can have a "Fallback" store that will fall through different layers of another restource stores. For example the top priority is file system store with the path `Assets/Textures/` (relative to the game directory). If that folder doesn't exist or doesn't contain the file, it will move down to AssemblyStream store which reads it from resources included in the assembly. This in theory allows players to overwrite the game resources (or even the built-in engine resources), allowing for very easy soft modding.

Modifiers like `.MakeCached()` or `.MakeAsync()` just wrap the fallback store in another stores:
- `CachedResourceStore` stores the result in a cache so consecutive lookups dont have to read from file system for example which is expensive
- `AsyncResourceStore` allows users to use `store.GetAsync(resource)` to get resource asynchronously without blocking the update or render thread
- `DeferredResourceStore` prevents resources which rely on the engine to be in certain state from loading until that state is reach (i.e shaders and textures waiting until graphics are initialized)

### Threading
- Yet again inspired by osu!framework but I do think this threading model should be the default these days. How many times I see games lag when loading a level or the music lag when the game is doing a lot or inputs being slowed down due to rendering and its very idk frustrating to me I guess?
- The engine is split into `Render`, `Update`, `Input` and `Audio` threads. Each thread runs at its own update rate. By default render is 240hz, update is 480hz and audio and input are 1000hz
  - Do note that for audio, most systems do not support pushing audio frames that fast so we do have to interpolate but this high of an update rate is important for example in rhythm games
- Each thread also has property to set its "Inactive" update rate (60 for all threads except audio), which just means its update rate when the window is not focsued. (this behaviour can be disable obviously)

### Resource Parsing/Loading
- For most resources, this is defined in [ResourceLoader](https://github.com/SynesthesiaDev/Synesthesia/blob/befa030bc4aa0f3d53075c88a38b061d095133d3/Synesthesia.Engine/Resources/ResourceLoaders.cs) and I do use [StbImage](https://github.com/nothings/stb/blob/master/stb_image.h) for image parsing but for pretty much everything else I made my own parser.

### Shaders
The shader format is not traditional and combined into single file due to the limitation of ResourceStores loading stuff by file and Shader obviously requires both vertex and fragment shaders.
I decided that putting it into a single file like for example Godot does would be the best approach. At the top its `Kind` is defined (3D or 2D) by doing `#pragma Kind Three` and then you seperate the actual shader codes by using `#pragma vertex` or `#pragma fragment`

```glsl
#pragma Kind Three
#pragma vertex
// vertex shader code...
#pragma fragment
// fragment shader code...
```
### Parsing and loading .obj files
Loading of meshes (`.obj` files) was rough tho as its very old format and not sequentially packed. It took me a while to understand it and learn it but in the end it comes down to it having **4 different tables**: 
  - `v` which defines vertices
  - `vt` which defines texture coordinates
  - `vn` which defines directions
  - `f` which defines instructions on how to combine previous 3 tables into faces 

Notably, it is 1-indexed because the original point of obj files was for humans to write them (crazy to even think about in these times huh)

_You can take a look at my [tldraw](https://offline.tldraw.com/) notes where I have visualized it [here](https://github.link_to.the/notes/)_

![img.png](img.png)
_(sorry for my horrible handwriting)_

Last updated Thu 29/Oct/2026 3am