// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Bindable;

namespace SynesthesiaDev.Game.Entities;

public abstract class Entity : KinematicDrawable
{
    public bool Invulnerable { get; set; } = false;

    public abstract BindableDouble Health { get; }

    public long BurningTime { get; set; } = 0L;

    public long AcidTime { get; set; } = 0L;

    public bool IsBurning => BurningTime <= 0;

    public float JumpHeight { get; set; } = 0.2f;

    public virtual float Speed { get; set; } = 1f;

}
