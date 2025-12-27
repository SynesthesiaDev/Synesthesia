using System.Numerics;

namespace Synesthesia.Engine.Animation;

public abstract class Transform<T>
{
    public abstract T Apply(T startValue, T endValue, float progress);
}

public static class Transform
{
    public static readonly Transform<float> Float = new FloatTranform((start, end, progress) => start + (end - start) * progress);
    public static readonly Transform<int> Int = new IntTransform((start, end, progress) => (int)(start + (end - start) * progress));
    public static readonly Transform<long> Long = new LongTransform((start, end, progress) => (long)(start + (end - start) * progress));
    public static readonly Transform<Vector2> Vector2 = new Vector2Transform((start, end, progress) => new Vector2(start.X + (end.X - start.X) * progress, start.Y + (end.Y - start.Y) * progress));

    public class FloatTranform(Func<float, float, float, float> transform) : Transform<float>
    {
        public override float Apply(float startValue, float endValue, float progress) => transform(startValue, endValue, progress);
    }

    public class IntTransform(Func<int, int, float, int> transform) : Transform<int>
    {
        public override int Apply(int startValue, int endValue, float progress)
        {
            return transform(startValue, endValue, progress);
        }
    }

    public class Vector2Transform(Func<Vector2, Vector2, float, Vector2> transform) : Transform<Vector2>
    {
        public override Vector2 Apply(Vector2 startValue, Vector2 endValue, float progress)
        {
            return transform(startValue, endValue, progress);
        }
    }

    public class LongTransform(Func<long, long, float, long> transform) : Transform<long>
    {
        public override long Apply(long startValue, long endValue, float progress)
        {
            return transform(startValue, endValue, progress);
        }
    }
}