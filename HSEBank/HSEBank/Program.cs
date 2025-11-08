namespace HSEBank;

public abstract class Program
{
    public static void Main()
    {
        var container = Bootstrapper.Configure();
        var ui = new ConsoleUi(container);
        ui.Start();
    }
}