// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using Synesthesia.Engine.Graphics.Three;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Physics.Three;

public class PhysicsBody3D : CompositeDrawable3D
{
    public PhysicsType Type { get; set; } = PhysicsType.Static;
    public float Mass { get; set; } = 1f;

    private PhysicsContainer3D physicsContainer = null!;
    private BodyHandle bodyHandle = new BodyHandle(-1);
    private StaticHandle staticHandle = new StaticHandle(-1);

    private RigidPose previousPose;
    private RigidPose currentPose;
    private int lastTimestepCount = -1;

    protected override void OnLoading()
    {
        base.OnLoading();
        physicsContainer = FindParentPhysicsContainer() ?? throw new InvalidOperationException("Physics Body must be child of `PhysicsContainer3D`");

        UpdateDrawMatrix();

        var simulation = physicsContainer.Simulation;
        var box = new Box(Width, Height, Length);
        var shapeIndex = simulation.Shapes.Add(in box);

        var pos = DrawMatrix.Matrix.Translation;
        var rot = RawRotation;

        currentPose = new RigidPose(pos, rot);
        previousPose = currentPose;

        if (Type == PhysicsType.Static)
        {
            staticHandle = simulation.Statics.Add(new StaticDescription(pos, rot, shapeIndex));
        }
        else if (Type == PhysicsType.Kinematic)
        {
            bodyHandle = simulation.Bodies.Add(BodyDescription.CreateKinematic(new RigidPose(pos, rot), new BodyVelocity(Vector3.Zero), shapeIndex, 0.01f));
        }
        else
        {
            var inertia = box.ComputeInertia(Mass);
            bodyHandle = simulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(pos, rot), inertia, shapeIndex, 0.01f));
        }
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (Type == PhysicsType.Kinematic)
        {
            var bodyRef = physicsContainer.Simulation.Bodies.GetBodyReference(bodyHandle);
            bodyRef.Pose.Position = DrawMatrix.Matrix.Translation;
            bodyRef.Pose.Orientation = RawRotation;
        }
        else if (Type == PhysicsType.Dynamic)
        {
            var sim = physicsContainer.Simulation;
            var bodyRef = sim.Bodies.GetBodyReference(bodyHandle);

            if (bodyRef.Awake)
            {
                if (physicsContainer.StepCount != lastTimestepCount)
                {
                    previousPose = currentPose;
                    currentPose = bodyRef.Pose;
                    lastTimestepCount = physicsContainer.StepCount;
                }
            }
        }

        base.OnUpdate(frameInfo);
    }

    public PhysicsContainer3D? FindParentPhysicsContainer()
    {
        var currentParent = Parent;

        while (currentParent != null)
        {
            if (currentParent is PhysicsContainer3D physContainer) return physContainer;
            currentParent = currentParent.Parent;
        }

        return null;
    }

    protected override void UpdateDrawMatrix()
    {
        if (Type == PhysicsType.Dynamic && IsLoaded)
        {
            float alpha = physicsContainer.InterpolationAlpha;

            var interpolatedPos = Vector3.Lerp(previousPose.Position, currentPose.Position, alpha);
            var interpolatedRot = Quaternion.Slerp(previousPose.Orientation, currentPose.Orientation, alpha);

            Position = interpolatedPos;
            RawRotation = interpolatedRot;
        }

        base.UpdateDrawMatrix();

    }
}
