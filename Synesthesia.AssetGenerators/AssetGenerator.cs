using Synesthesia.AssetGenerators.Generators;
using Synesthesia.Engine;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Textures;
using Synesthesia.Engine.Resources;
using Synesthesia.Utils;

namespace Synesthesia.AssetGenerators;

internal static class AssetGenerator
{
    private static void Main(string[] args)
    {
        var game = new GameBuilder().Build();

        var fontAtlasGenerator = new FontAtlasGenerator();

        game.OnInitialized.Subscribe(_ =>
        {
            var dependencyContainer = DependencyContainer.Get<IResourceStore<Font>>();
            var font = dependencyContainer.Get("Synesthesia.Resources.Fonts.Quicksand-Regular.ttf");
            var fonts = Maps.Of(
                ("Quicksand-Regular", font)
            );

            fontAtlasGenerator.Run(fonts);
        });

        game.Run();
    }
}
