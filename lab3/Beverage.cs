namespace Lab3;

public interface IBeverage
{
    string Ingredients();

    float Cost();
}

public class Espresso : IBeverage
{
    public float Cost() => 2;

    public string Ingredients() => "Coffee";
}

public class Cocoa : IBeverage
{
    public float Cost() => 1.5f;

    public string Ingredients() => "Cocoa";
}

public class Chocolate : IBeverage
{
    public float Cost() => 3f;

    public string Ingredients() => "Chocolate";
}

internal abstract class ExtraIngredient(IBeverage beverage) : IBeverage
{
    public virtual float Cost() => beverage.Cost();

    public virtual string Ingredients() => beverage.Ingredients();
}

internal class HotWater(IBeverage beverage) : ExtraIngredient(beverage)
{
    public override string Ingredients()
    {
        return $"{base.Ingredients()}, Hot Water";
    }
}

internal enum Amount
{
    Lot,
    Little
}

internal class Milk(IBeverage beverage, Amount amount) : ExtraIngredient(beverage)
{
    private Amount Size { set; get; } = amount;

    public override string Ingredients()
    {
        return $"{base.Ingredients()}, Milk ({Size})";
    }

    public override float Cost()
    {
        return base.Cost() + Size switch
        {
            Amount.Lot => 1.7f,
            _ => 0.9f
        };
    }
}

internal class MilkFoam(IBeverage beverage, Amount amount) : ExtraIngredient(beverage)
{
    private Amount Size { set; get; } = amount;

    public override string Ingredients()
    {
        return $"{base.Ingredients()}, Milk Foam ({Size})";
    }

    public override float Cost()
    {
        return base.Cost() + Size switch
        {
            Amount.Lot => 0.4f,
            _ => 0.2f
        };
    }
}

internal class Sugar(IBeverage beverage, Amount amount) : ExtraIngredient(beverage)
{
    private Amount Size { set; get; } = amount;

    public override string Ingredients()
    {
        return $"{base.Ingredients()}, Sugar ({Size})";
    }

    public override float Cost()
    {
        return base.Cost() + Size switch
        {
            Amount.Lot => 1f,
            _ => 0.5f
        }; ;
    }
}