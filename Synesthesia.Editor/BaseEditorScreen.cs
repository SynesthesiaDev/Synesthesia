using System.Numerics;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.Editor;

public class BaseEditorScreen : Screen
{
    protected override void OnLoading()
    {
        Children =
        [
            new FillFlowContainer2d
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = Direction.Vertical,
                Children =
                [
                    new TextDrawable
                    {
                        Text = "Base Editor Screen",
                        Color = Color.White
                    },
                    new DefaultButton
                    {
                        Size = new Vector2(120, 40),
                        Text = "Edit Screen",
                        OnClick = () =>
                        {
                            var editor = DependencyContainer.Get<Editor>();
                            editor.ScreenStack.Push(new EditorLevelMetaScreen(editor.CurrentLevel!));
                        }
                    }
                ]
            }
        ];
    }
}