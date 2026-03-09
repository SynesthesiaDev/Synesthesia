using System.Numerics;
using Common.Pooling;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Renderer;
using Synesthesia.Engine.Graphics.Shaders;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Input.Events;
using Synesthesia.Engine.Resources;
using Synesthesia.Engine.Utility;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Graphics.Two.Drawables;

public class CompositeDrawable2d : Drawable2d
{
    protected internal List<Drawable2d> InternalChildren = [];

    [Resolved]
    private IRenderer renderer = null!;

    [Resolved]
    private IResourceStore<Shader> shaderStore = null!;

    private Shader? borderShader;

    private Texture2D? borderTexture;

    public ComplexColor BorderColor
    {
        get;
        set
        {
            if (field.Equals(value)) return;

            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = ComplexColor.BLACK;

    public int BorderThickness
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = 0;


    public BorderType BorderType
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = BorderType.Outset;

    public float CornerRadius
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.DrawNode | Invalidation.Geometry);
        }
    } = 0f;

    public bool Masking
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            Invalidate(Invalidation.All);
        }
    } = false;

    public Vector4 AutoSizePadding
    {
        get => autoSizePadding;
        set
        {
            if (autoSizePadding == value) return;
            autoSizePadding = value;
            Invalidate(Invalidation.Size | Invalidation.Geometry | Invalidation.Layout);
        }
    }

    private readonly object childrenLock = new();
    private Vector4 autoSizePadding = new(0);

    public IEnumerable<Drawable2d> Children
    {
        get => InternalChildren;
        set
        {
            lock (childrenLock)
            {
                if (InternalChildren.Count > 0)
                {
                    foreach (var oldChild in InternalChildren)
                    {
                        oldChild.Parent = null;
                        oldChild.Dispose();
                    }

                    InternalChildren.Clear();
                }

                InternalChildren = value.ToList();
                foreach (var child in value)
                {
                    child.Parent = this;
                    if (IsLoaded)
                    {
                        child.Load();
                    }
                }

                Invalidate(Invalidation.All);
            }
        }
    }

    protected internal void InvalidateChildren(Invalidation flags)
    {
        foreach (var internalChild in InternalChildren)
        {
            internalChild.Invalidate(flags);
        }
    }

    protected override void InternalLoadComplete()
    {
        lock (childrenLock)
        {
            foreach (var internalChild in InternalChildren)
            {
                internalChild.Load();
            }
        }

        UpdateLayout();

        base.InternalLoadComplete();
    }

    protected internal void UpdateHoverState(MouseMoveInputEvent e)
    {
        var handled = false;

        for (var i = InternalChildren.Count - 1; i >= 0; i--)
        {
            var child = InternalChildren[i];
            if (!child.CanHandleInput) continue;

            var containsMouse = child.Contains(e.Position);

            if (handled | !containsMouse)
            {
                if (child.IsHovered)
                {
                    child.IsHovered = false;
                    child.OnHoverLost(e);
                }
            }
            else
            {
                if (!child.IsHovered)
                {
                    if (child.OnHover(e))
                    {
                        child.IsHovered = true;
                        handled = true;
                    }
                }
                else
                {
                    handled = true;
                }
            }

            if (child is CompositeDrawable2d composite)
            {
                composite.UpdateHoverState(e);
            }
        }
    }

    protected internal void UpdatePointInputState(PointInput e, bool down)
    {
        for (var i = InternalChildren.Count - 1; i >= 0; i--)
        {
            var child = InternalChildren[i];
            if (!child.CanHandleInput) continue;

            if (down && child is { IsMouseDown: false, IsHovered: true } && child.OnMouseDown(e))
            {
                child.IsMouseDown = true;
            }

            if (!down && child.IsMouseDown)
            {
                child.IsMouseDown = false;
                child.OnMouseUp(e);
            }

            if (child is CompositeDrawable2d drawable2d)
            {
                drawable2d.UpdatePointInputState(e, down);
            }
        }
    }

    protected internal void UpdateActionBindingState(ActionBinding e, bool down)
    {
        foreach (var child in InternalChildren.Filter(c => c.CanHandleInput).Reversed())
        {
            var handled = down && child.OnActionBindingDown(e);

            if (!down) child.OnActionBindingUp(e);

            if (handled) continue;

            if (child is CompositeDrawable2d drawable2d)
            {
                drawable2d.UpdateActionBindingState(e, down);
            }
        }
    }

    protected internal void UpdateScrollWheelState(MouseScrollWheelInputEvent e)
    {
        foreach (var child in InternalChildren.Filter(c => c.CanHandleInput && c.Contains(InputManager.MousePosition)).Reversed())
        {
            var handled = child.OnMouseWheel(e.Delta);

            if (handled) continue;

            if (child is CompositeDrawable2d drawable2d)
            {
                drawable2d.UpdateScrollWheelState(e);
            }
        }
    }

    protected internal void UpdateKeyState(KeyboardKey e, bool down)
    {
        for (int i = InternalChildren.Count - 1; i >= 0; i--)
        {
            var child = InternalChildren[i];
            if (!child.CanHandleInput) continue;

            var handled = down && child.OnKeyDown(e);
            if (!down) child.OnKeyUp(e);

            if (handled) break;

            if (child is CompositeDrawable2d composite)
                composite.UpdateKeyState(e, down);
        }
    }

    public void AddChild(Drawable2d child)
    {
        lock (childrenLock)
        {
            InternalChildren.Add(child);
            child.Parent = this;
            child.Load();
        }
    }

    public void RemoveChild(Drawable2d child)
    {
        lock (childrenLock)
        {
            InternalChildren.Remove(child);
            child.Dispose();
            Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        Snapshot<Drawable2d> snapshot;
        lock (childrenLock)
        {
            snapshot = Snapshot.Rent(InternalChildren);
        }

        try
        {
            for (int i = 0; i < snapshot.Count; i++)
            {
                snapshot.Array[i].OnUpdate(frameInfo);
            }
        }

        finally
        {
            snapshot.Return();
        }

        base.OnUpdate(frameInfo);
    }

    protected override void OnLayout(Invalidation dirty)
    {
        base.OnLayout(dirty);

        if (dirty.HasFlagFast(Invalidation.Size) && AutoSizeAxes != Axes.None)
        {
            UpdateAutoSize();
        }
    }

    protected virtual void UpdateAutoSize()
    {
        var childrenSize = GetChildrenSize();

        if (AutoSizeAxes.HasFlagFast(Axes.X)) Width = childrenSize.X + AutoSizePadding.X + AutoSizePadding.Z;
        if (AutoSizeAxes.HasFlagFast(Axes.Y)) Height = childrenSize.Y + AutoSizePadding.Y + AutoSizePadding.W;
    }

    protected override void OnDraw2d()
    {
        var snapshot = Snapshot.Rent(InternalChildren);

        var doMasking = Masking;

        if (BorderThickness > 0 && BorderType == BorderType.Inset)
        {
            drawBorder();
        }

        if (doMasking)
        {
            OpenGl.glClear(OpenGl.GL_STENCIL_BUFFER_BIT);
            Rlgl.BeginStencil();
            Rlgl.BeginStencilMask();
            OpenGl.glAlphaFunc(OpenGl.GL_GREATER, 0.05f);

            var maskRect = new Rectangle(0, 0, Size.X, Size.Y);

            if (CornerRadius > 0)
            {
                var roundness = Math.Clamp(CornerRadius * 2 / Math.Min(Size.X, Size.Y), 0f, 1f);
                Raylib.DrawRectangleRounded(maskRect, roundness, 32, Color.White);
            }
            else
            {
                Raylib.DrawRectangleRec(maskRect, Color.White);
            }

            Rlgl.EndStencilMask();
        }

        try
        {
            for (int i = 0; i < snapshot.Count; i++)
            {
                snapshot.Array[i].OnDraw();
            }
        }
        finally
        {
            if (doMasking)
            {
                Rlgl.EndStencil();
            }

            snapshot.Return();
        }

        if (BorderThickness > 0 && BorderType == BorderType.Outset)
        {
            drawBorder();
        }
    }

    private void drawBorder()
    {
        borderShader ??= shaderStore.Get("Synesthesia.Resources.Shaders.border.fsh");
        borderTexture ??= new Texture2D
        {
            Id = Rlgl.GetTextureIdDefault(),
            Width = 1,
            Height = 1,
            Mipmaps = 1,
            Format = PixelFormat.UncompressedR8G8B8A8
        };

        var shader = borderShader.NativeShader;

        Raylib.SetShaderValue(shader,
            Raylib.GetShaderLocation(shader, "topLeftColor"),
            BorderColor.TopLeft.ToNormalizedVector(),
            ShaderUniformDataType.Vec4);

        Raylib.SetShaderValue(shader,
            Raylib.GetShaderLocation(shader, "topRightColor"),
            BorderColor.TopRight.ToNormalizedVector(),
            ShaderUniformDataType.Vec4);

        Raylib.SetShaderValue(shader,
            Raylib.GetShaderLocation(shader, "bottomLeftColor"),
            BorderColor.BottomLeft.ToNormalizedVector(),
            ShaderUniformDataType.Vec4);

        Raylib.SetShaderValue(shader,
            Raylib.GetShaderLocation(shader, "bottomRightColor"),
            BorderColor.BottomRight.ToNormalizedVector(),
            ShaderUniformDataType.Vec4);

        Raylib.SetShaderValue(shader,
            Raylib.GetShaderLocation(shader, "size"),
            Size,
            ShaderUniformDataType.Vec2);

        Raylib.SetShaderValue(shader,
            Raylib.GetShaderLocation(shader, "borderThickness"),
            BorderThickness,
            ShaderUniformDataType.Int);

        Raylib.SetShaderValue(shader,
            Raylib.GetShaderLocation(shader, "cornerRadius"),
            CornerRadius,
            ShaderUniformDataType.Float);

        Raylib.BeginShaderMode(shader);
        Raylib.DrawTexturePro(borderTexture.Value, new Rectangle(0, 0, 1, 1), new Rectangle(0, 0, Size.X, Size.Y), Vector2.Zero, 0.0f, Color.White);
        Raylib.EndShaderMode();
    }

    protected override void Dispose(bool isDisposing)
    {
        lock (childrenLock)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];

                if (child is IPooledObject { IsPooled: true, ReturnAction: not null } pooledObject)
                {
                    pooledObject.ReturnAction.Invoke(pooledObject);
                }
                else
                {
                    child.Dispose();
                }
            }

            InternalChildren.Clear();
        }

        base.Dispose(isDisposing);
    }

    public Vector2 GetChildrenSize()
    {
        if (InternalChildren.Count == 0) return Vector2.Zero;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        for (int i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[i];
            var scaledSize = child.Size * child.Scale;

            minX = Math.Min(minX, child.Position.X);
            minY = Math.Min(minY, child.Position.Y);
            maxX = Math.Max(maxX, child.Position.X + scaledSize.X);
            maxY = Math.Max(maxY, child.Position.Y + scaledSize.Y);
        }

        return new Vector2(maxX - minX, maxY - minY);
    }

    public List<Drawable2d> GetFlattenedChildrenList()
    {
        var list = new List<Drawable2d>();
        getChildrenRecursive(this, list);
        return list;
    }

    private static void getChildrenRecursive(CompositeDrawable2d compositeDrawable2d, List<Drawable2d> outList)
    {
        foreach (var child in compositeDrawable2d.InternalChildren)
        {
            outList.Add(child);
            if (child is CompositeDrawable2d compositeChild)
            {
                getChildrenRecursive(compositeChild, outList);
            }
        }
    }

    public Animation<int> ResizeBorder(int newSize, long duration, Easing easing)
    {
        return TransformTo(nameof(BorderThickness), BorderThickness, newSize, duration, easing, Transforms.INT, thickness => { BorderThickness = thickness; });
    }

    public Animation<ComplexColor> FadeBorderColorTo(ComplexColor newColor, long duration, Easing easing)
    {
        return TransformTo(nameof(BorderColor), BorderColor, newColor, duration, easing, Transforms.COMPLEX_COLOR, borderColor => { BorderColor = borderColor; });
    }
}
