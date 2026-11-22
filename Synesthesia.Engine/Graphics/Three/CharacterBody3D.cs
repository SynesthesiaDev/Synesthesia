using System.Numerics;
using System.Runtime.InteropServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using Synesthesia.Engine.Physics.Three;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Graphics.Three;

public abstract class CharacterBody3D : CompositeDrawable3D
{
    public Vector3 Velocity { get; set; } = Vector3.Zero;
    public bool IsGrounded { get; protected set; }
    public bool UseGravity { get; set; } = true;
    public float Gravity { get; set; } = -20f;

    public float CapsuleRadius { get; set; } = 0.35f;
    public float CapsuleHeight { get; set; } = 1.8f;

    protected PhysicsContainer3D PhysicsContainer = null!;
    protected BodyHandle BodyHandle;
    private Capsule capsuleShape;

    protected override void OnLoading()
    {
        PhysicsContainer = FindParentPhysicsContainer() ?? throw new InvalidOperationException("Character must be inside a PhysicsContainer3D!");
        var sim = PhysicsContainer.Simulation;

        float cylinderLength = CapsuleHeight - (CapsuleRadius * 2);
        capsuleShape = new Capsule(CapsuleRadius, cylinderLength);
        var shapeIndex = sim.Shapes.Add(in capsuleShape);

        var startPos = DrawMatrix.Matrix.Translation;
        startPos.Y += 0.1f;

        BodyHandle = sim.Bodies.Add(BodyDescription.CreateKinematic(
            new RigidPose(startPos, Quaternion.Identity),
            new BodyVelocity(Vector3.Zero),
            shapeIndex,
            0.01f
        ));

        base.OnLoading();
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        float dt = frameInfo.DeltaSeconds;
        var sim = PhysicsContainer.Simulation;

        if (UseGravity && !IsGrounded)
            Velocity = Velocity with { Y = Velocity.Y + Gravity * dt };
        else if (IsGrounded && Velocity.Y < 0)
            Velocity = Velocity with { Y = 0f };

        OnCharacterUpdate(frameInfo);

        var bodyRef = sim.Bodies.GetBodyReference(BodyHandle);
        var displacement = Velocity * dt;

        var hitHandler = new CharacterSweepHandler(BodyHandle)
        {
            OriginalDisplacement = displacement,
            ResolvedDisplacement = displacement // Default to moving normally if we hit nothing
        };

        sim.Sweep(
            in capsuleShape,
            in bodyRef.Pose,
            new BodyVelocity(displacement),
            1.0f,
            sim.BufferPool,
            ref hitHandler
        );

        // Apply movement
        bodyRef.Pose.Position += hitHandler.ResolvedDisplacement;

        checkGrounded(sim, bodyRef);

        // SYNC VISUAL POSITION!
        Position = bodyRef.Pose.Position;

        base.OnUpdate(frameInfo);
    }

    private const float ground_probe = 0.1f;  // how far below the feet counts as grounded
    private const float skin = 0.02f;         // resting gap

    private void checkGrounded(Simulation sim, BodyReference bodyRef)
    {
        float half = CapsuleHeight / 2;
        var origin = bodyRef.Pose.Position;
        var handler = new GroundRayHandler(BodyHandle);
        sim.RayCast(in origin, new Vector3(0, -1, 0), half + ground_probe, ref handler);

        IsGrounded = handler.HitSomething && Velocity.Y <= 0;

        if (IsGrounded)
        {
            // Sit exactly skin above the surface; no more sinking/pushing
            bodyRef.Pose.Position = origin with { Y = origin.Y - handler.Distance + half + skin };
        }
    }

    protected abstract void OnCharacterUpdate(FrameInfo frameInfo);

    protected PhysicsContainer3D? FindParentPhysicsContainer()
    {
        var currentParent = Parent;
        while (currentParent != null)
        {
            if (currentParent is PhysicsContainer3D physContainer) return physContainer;
            currentParent = currentParent.Parent;
        }
        return null;
    }

    private struct GroundRayHandler(BodyHandle handle) : IRayHitHandler
    {
        public bool HitSomething = false;
        public float Distance = float.MaxValue;

        public readonly bool AllowTest(CollidableReference collidable)
        {
            if (collidable.Mobility == CollidableMobility.Static) return true;
            return collidable.BodyHandle != handle;
        }

        public readonly bool AllowTest(CollidableReference collidable, int childIndex) => AllowTest(collidable);

        public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
        {
            if (t >= Distance) return;
            HitSomething = true;
            Distance = t;
            maximumT = t;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    private struct CharacterSweepHandler(BodyHandle handle) : ISweepHitHandler
    {
        public Vector3 OriginalDisplacement = Vector3.Zero;
        public Vector3 ResolvedDisplacement = Vector3.Zero;

        public readonly bool AllowTest(CollidableReference collidable)
        {
            if (collidable.Mobility == CollidableMobility.Static) return true;
            return collidable.BodyHandle != handle;
        }

        public readonly bool AllowTest(CollidableReference collidable, int child) => AllowTest(collidable);

        public void OnHit(ref float maximumT, float t, in Vector3 hitLocation, in Vector3 hitNormal, CollidableReference collidable)
        {
            var traveled = OriginalDisplacement * t;
            var remaining = OriginalDisplacement * (1f - t);
            var slide = remaining - Vector3.Dot(remaining, hitNormal) * hitNormal;
            var pushOut = hitNormal * 0.01f;

            ResolvedDisplacement = traveled + slide + pushOut;
            maximumT = 0;
        }

        public void OnHitAtZeroT(ref float maximumT, CollidableReference collidable)
        {
            // If we are moving UP, don't cancel the jump!
            if (OriginalDisplacement.Y > 0)
            {
                ResolvedDisplacement = OriginalDisplacement;
            }
            else
            {
                // Otherwise, we are on the ground, so cancel downward movement
                ResolvedDisplacement = new Vector3(OriginalDisplacement.X, 0, OriginalDisplacement.Z);
            }
            maximumT = 0;
        }
    }
}
