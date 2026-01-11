namespace Synesthesia.Engine.Animations.Easings;

public readonly struct EasingFunction(Easing easing) : IEasingFunction
{
    private const double ElasticConst = 2 * Math.PI / .3;
    private const double ElasticConst2 = .3 / 4;

    private const double BackConst = 1.70158;
    private const double BackConst2 = BackConst * 1.525;

    private const double BounceConst = 1 / 2.75;

    private static readonly double ExpoOffset = Math.Pow(2, -10);
    private static readonly double ElasticOffsetFull = Math.Pow(2, -11);
    private static readonly double ElasticOffsetHalf = Math.Pow(2, -10) * Math.Sin((.5 - ElasticConst2) * ElasticConst);
    private static readonly double ElasticOffsetQuarter = Math.Pow(2, -10) * Math.Sin((.25 - ElasticConst2) * ElasticConst);
    private static readonly double InOutElasticOffset = Math.Pow(2, -10) * Math.Sin((1 - ElasticConst2 * 1.5) * ElasticConst / 1.5);

    public double ApplyEasing(double time)
    {
        switch (easing)
        {
            case Easing.None:
            default:
                return time;

            case Easing.In:
            case Easing.InQuad:
                return time * time;

            case Easing.Out:
            case Easing.OutQuad:
                return time * (2 - time);

            case Easing.InOutQuad:
                if (time < .5) return time * time * 2;

                return --time * time * -2 + 1;

            case Easing.InCubic:
                return time * time * time;

            case Easing.OutCubic:
                return --time * time * time + 1;

            case Easing.InOutCubic:
                if (time < .5) return time * time * time * 4;

                return --time * time * time * 4 + 1;

            case Easing.InQuart:
                return time * time * time * time;

            case Easing.OutQuart:
                return 1 - --time * time * time * time;

            case Easing.InOutQuart:
                if (time < .5) return time * time * time * time * 8;

                return --time * time * time * time * -8 + 1;

            case Easing.InQuint:
                return time * time * time * time * time;

            case Easing.OutQuint:
                return --time * time * time * time * time + 1;

            case Easing.InOutQuint:
                if (time < .5) return time * time * time * time * time * 16;

                return --time * time * time * time * time * 16 + 1;

            case Easing.InSine:
                return 1 - Math.Cos(time * Math.PI * .5);

            case Easing.OutSine:
                return Math.Sin(time * Math.PI * .5);

            case Easing.InOutSine:
                return .5 - .5 * Math.Cos(Math.PI * time);

            case Easing.InExpo:
                return Math.Pow(2, 10 * (time - 1) + ExpoOffset * (time - 1));

            case Easing.OutExpo:
                return -Math.Pow(2, -10 * time) + 1 + ExpoOffset * time;

            case Easing.InOutExpo:
                if (time < .5) return .5 * (Math.Pow(2, 20 * time - 10) + ExpoOffset * (2 * time - 1));

                return 1 - .5 * (Math.Pow(2, -20 * time + 10) + ExpoOffset * (-2 * time + 1));

            case Easing.InCirc:
                return 1 - Math.Sqrt(1 - time * time);

            case Easing.OutCirc:
                return Math.Sqrt(1 - --time * time);

            case Easing.InOutCirc:
                if ((time *= 2) < 1) return .5 - .5 * Math.Sqrt(1 - time * time);

                return .5 * Math.Sqrt(1 - (time -= 2) * time) + .5;

            case Easing.InElastic:
                return -Math.Pow(2, -10 + 10 * time) * Math.Sin((1 - ElasticConst2 - time) * ElasticConst) + ElasticOffsetFull * (1 - time);

            case Easing.OutElastic:
                return Math.Pow(2, -10 * time) * Math.Sin((time - ElasticConst2) * ElasticConst) + 1 - ElasticOffsetFull * time;

            case Easing.OutElasticHalf:
                return Math.Pow(2, -10 * time) * Math.Sin((.5 * time - ElasticConst2) * ElasticConst) + 1 - ElasticOffsetHalf * time;

            case Easing.OutElasticQuarter:
                return Math.Pow(2, -10 * time) * Math.Sin((.25 * time - ElasticConst2) * ElasticConst) + 1 - ElasticOffsetQuarter * time;

            case Easing.InOutElastic:
                if ((time *= 2) < 1)
                {
                    return -.5 * (Math.Pow(2, -10 + 10 * time) * Math.Sin((1 - ElasticConst2 * 1.5 - time) * ElasticConst / 1.5)
                                  - InOutElasticOffset * (1 - time));
                }

                return .5 * (Math.Pow(2, -10 * --time) * Math.Sin((time - ElasticConst2 * 1.5) * ElasticConst / 1.5)
                             - InOutElasticOffset * time) + 1;

            case Easing.InBack:
                return time * time * ((BackConst + 1) * time - BackConst);

            case Easing.OutBack:
                return --time * time * ((BackConst + 1) * time + BackConst) + 1;

            case Easing.InOutBack:
                if ((time *= 2) < 1) return .5 * time * time * ((BackConst2 + 1) * time - BackConst2);

                return .5 * ((time -= 2) * time * ((BackConst2 + 1) * time + BackConst2) + 2);

            case Easing.InBounce:
                time = 1 - time;
                if (time < BounceConst)
                    return 1 - 7.5625 * time * time;
                if (time < 2 * BounceConst)
                    return 1 - (7.5625 * (time -= 1.5 * BounceConst) * time + .75);
                if (time < 2.5 * BounceConst)
                    return 1 - (7.5625 * (time -= 2.25 * BounceConst) * time + .9375);

                return 1 - (7.5625 * (time -= 2.625 * BounceConst) * time + .984375);

            case Easing.OutBounce:
                if (time < BounceConst)
                    return 7.5625 * time * time;
                if (time < 2 * BounceConst)
                    return 7.5625 * (time -= 1.5 * BounceConst) * time + .75;
                if (time < 2.5 * BounceConst)
                    return 7.5625 * (time -= 2.25 * BounceConst) * time + .9375;

                return 7.5625 * (time -= 2.625 * BounceConst) * time + .984375;

            case Easing.InOutBounce:
                if (time < .5) return .5 - .5 * new EasingFunction(Easing.OutBounce).ApplyEasing(1 - time * 2);

                return new EasingFunction(Easing.OutBounce).ApplyEasing((time - .5) * 2) * .5 + .5;

            case Easing.OutPow10:
                return --time * Math.Pow(time, 10) + 1;
        }
    }
}