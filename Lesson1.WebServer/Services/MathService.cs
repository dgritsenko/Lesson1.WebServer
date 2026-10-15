namespace Lesson1.WebServer.Services;

public sealed class MathService : IMathService
{
    public double Add(double first, double second) => first + second;

    public bool TryDivide(double dividend, double divisor, out double result)
    {
        if (divisor == 0d)
        {
            result = double.NaN;
            return false;
        }

        result = dividend / divisor;
        return double.IsFinite(result);
    }
}
