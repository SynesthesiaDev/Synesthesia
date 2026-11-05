// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Physics.Three;

public class PhysicsContainer3D : CompositeDrawable3D
{
    private const float fixed_time_step = 1f / 420f;
    private float accumulator;

    public Simulation Simulation { get; private set; } = null!;
    public int StepCount { get; private set; }
    public bool UpdatePhysics { get; set; } = true;
    public float InterpolationAlpha => Math.Clamp(accumulator / fixed_time_step, 0f, 1f);

    protected override void OnLoading()
    {
        Simulation = Simulation.Create(
            new BufferPool(),
            new NarrowPhaseCallbacks(new SpringSettings(30, 1)),
            new PoseIntegratorCallbacks(new Vector3(0, -9.81f, 0)),
            new SolveDescription(8, 1)
        );

        base.OnLoading();
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (UpdatePhysics)
        {
            accumulator += frameInfo.DeltaSeconds;

            long frameStepTimeMicroseconds = 0;
            int stepsThisFrame = 0;

            while (accumulator >= fixed_time_step)
            {
                long startTicks = Stopwatch.GetTimestamp();

                Simulation.Timestep(fixed_time_step);

                long endTicks = Stopwatch.GetTimestamp();
                frameStepTimeMicroseconds += (long)(Stopwatch.GetElapsedTime(startTicks, endTicks).TotalMicroseconds);

                accumulator -= fixed_time_step;
                StepCount++;
                stepsThisFrame++;
            }

            PhysicsStatistics.Set(PhysicsStatistics.Type.StepTimeMicroseconds, (int)frameStepTimeMicroseconds);
            PhysicsStatistics.Set(PhysicsStatistics.Type.StepsThisFrame, stepsThisFrame);
            PhysicsStatistics.Set(PhysicsStatistics.Type.AwakeBodies, Simulation.Bodies.ActiveSet.Count);
            PhysicsStatistics.Set(PhysicsStatistics.Type.SleepingBodies, Simulation.Bodies.Sets.Length > 1 ? 1 : 0);
            PhysicsStatistics.Set(PhysicsStatistics.Type.StaticBodies, Simulation.Statics.Count);
        }

        base.OnUpdate(frameInfo);
    }
}
