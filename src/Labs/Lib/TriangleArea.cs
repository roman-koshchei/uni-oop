using Microsoft.AspNetCore.Components;
using System.Drawing;

namespace Labs.Lib;

public interface ITriangleArea
{
    double Calculate();
}

public class FormInput(string label)
{
    public string Label { get; set; } = label;
    public double Value { get; set; } = 0;
}

public abstract class TriangleAreaForm(params FormInput[] inputs)
{
    public abstract ITriangleArea CreateTriangleArea();

    protected List<FormInput> inputs = [.. inputs];

    public IReadOnlyList<FormInput> Inputs { get => inputs; }

    public string ResultMessage()
    {
        try
        {
            var triangleArea = CreateTriangleArea();
            var area = triangleArea.Calculate();
            return $"Area of triangle: {area}";
        }
        catch (TriangleAreaCalcException ex)
        {
            return ex.Message;
        }
        catch
        {
            return "Area can't be calculated. Please check input data";
        }
    }
}

public class TriangleAreaCalcException(string userMessage) : Exception(userMessage)
{ }

// Heights

public class TriangleAreaByHeights(double height, double side) : ITriangleArea
{
    public double Calculate() => 0.5 * side * height;
}

public class TriangleAreaByHeightsForm() : TriangleAreaForm(new FormInput("Height"), new FormInput("Side"))
{
    public override ITriangleArea CreateTriangleArea()
    {
        var height = inputs[0].Value;
        var side = inputs[1].Value;
        if (side <= 0 || height <= 0)
        {
            throw new TriangleAreaCalcException("Side and height must be positive numbers");
        }

        return new TriangleAreaByHeights(height, side);
    }
}

// Medians
public class TriangleAreaByMedians(double m1, double m2, double m3) : ITriangleArea
{
    public double Calculate()
    {
        double s = (m1 + m2 + m3) / 2;
        return (4.0 / 3.0) * Math.Sqrt(s * (s - m1) * (s - m2) * (s - m3));
    }
}

public class TriangleAreaByMediansForm() : TriangleAreaForm(new FormInput("Median 1"), new FormInput("Median 2"), new FormInput("Median 3"))
{
    public override ITriangleArea CreateTriangleArea()
    {
        var median1 = inputs[0].Value;
        var median2 = inputs[1].Value;
        var median3 = inputs[2].Value;
        if (median1 <= 0 || median2 <= 0 || median3 <= 0)
        {
            throw new TriangleAreaCalcException("Medians must be positive numbers");
        }
        return new TriangleAreaByMedians(median1, median2, median3);
    }
}

// Circles

public class TriangleAreaByCircles(double rInscribed, double rCircumscribed) : ITriangleArea
{
    public double Calculate() => rInscribed * rCircumscribed * 3;
}

public class TriangleAreaByCirclesForm() : TriangleAreaForm(new FormInput("Inscribed Radius"), new FormInput("Circumscribed Radius"))
{
    public override ITriangleArea CreateTriangleArea()
    {
        var rInscribed = inputs[0].Value;
        var rCircumscribed = inputs[1].Value;
        if (rInscribed <= 0 || rCircumscribed <= 0)
        {
            throw new TriangleAreaCalcException("Radius must be positive numbers");
        }
        if (rInscribed >= rCircumscribed)
        {
            throw new TriangleAreaCalcException("The inscribed circle radius must be smaller than the circumscribed circle radius");
        }
        return new TriangleAreaByCircles(rInscribed, rCircumscribed);
    }
}

// Coordinates

public class TriangleAreaByCoordinates(double x1, double y1, double x2, double y2, double x3, double y3) : ITriangleArea
{
    public double Calculate() => Math.Abs((x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0);
}

public class TriangleAreaByCoordinatesForm() : TriangleAreaForm(new FormInput("X1"), new FormInput("Y1"), new FormInput("X2"), new FormInput("Y2"), new FormInput("X3"), new FormInput("Y3"))
{
    public override ITriangleArea CreateTriangleArea()
    {
        var x1 = inputs[0].Value;
        var y1 = inputs[1].Value;
        var x2 = inputs[2].Value;
        var y2 = inputs[3].Value;
        var x3 = inputs[4].Value;
        var y3 = inputs[5].Value;
        return new TriangleAreaByCoordinates(x1, y1, x2, y2, x3, y3);
    }
}

// Bisectors

public class TriangleAreaByBisectors(double b1, double b2, double b3, double a, double b, double c) : ITriangleArea
{
    public double Calculate()
    {
        double semiPerimeter = (a + b + c) / 2;
        return (b1 * b2 * b3) / (4 * Math.Sqrt(semiPerimeter * (semiPerimeter - a) * (semiPerimeter - b) * (semiPerimeter - c)));
    }
}

public class TriangleAreaByBisectorsForm() : TriangleAreaForm(new FormInput("Bisector 1"), new FormInput("Bisector 2"), new FormInput("Bisector 3"), new FormInput("Side 1"), new FormInput("Side 2"), new FormInput("Side 3"))
{
    public override ITriangleArea CreateTriangleArea()
    {
        var b1 = inputs[0].Value;
        var b2 = inputs[1].Value;
        var b3 = inputs[2].Value;
        var a = inputs[3].Value;
        var b = inputs[4].Value;
        var c = inputs[5].Value;
        if (b1 <= 0 || b2 <= 0 || b3 <= 0 || a <= 0 || b <= 0 || c <= 0)
        {
            throw new TriangleAreaCalcException("Bisectors and sides must be positive numbers");
        }
        return new TriangleAreaByBisectors(b1, b2, b3, a, b, c);
    }
}