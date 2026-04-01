// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics.Two;

namespace Synesthesia.Engine.Components.Two.Barebones;

public abstract class TextboxCaret : CompositeDrawable2d
{
    public abstract void Show();
    public abstract void Hide();
}
