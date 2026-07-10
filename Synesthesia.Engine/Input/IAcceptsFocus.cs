// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics.Two;

namespace Synesthesia.Engine.Input;

public interface IAcceptsFocus
{
    Drawable2D OwningDrawable { get; }

    void OnFocusGained();

    void OnFocusLost();

    void OnTextTyped(string text)
    {
    }

    void OnWindowActiveChange(bool active)
    {
    }
}
