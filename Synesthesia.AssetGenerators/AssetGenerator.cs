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
            var fonts = Maps.Of(
                ("Quicksand-Regular", dependencyContainer.Get("Synesthesia.Resources.Fonts.Quicksand-Regular.ttf")),
                ("MapleMono-Regular", dependencyContainer.Get("Synesthesia.Resources.Fonts.MapleMono-Regular.ttf")),
                ("MapleMono-Light", dependencyContainer.Get("Synesthesia.Resources.Fonts.MapleMono-Light.ttf")),
                ("MapleMono-ExtraLight", dependencyContainer.Get("Synesthesia.Resources.Fonts.MapleMono-ExtraLight.ttf")),
                ("MapleMono-Thin", dependencyContainer.Get("Synesthesia.Resources.Fonts.MapleMono-Thin.ttf"))
            );

            fontAtlasGenerator.Run(fonts);
            game.Exit();
        });

        game.Run();
    }
}
