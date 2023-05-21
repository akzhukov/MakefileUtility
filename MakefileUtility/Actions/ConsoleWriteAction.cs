namespace MakefileUtility.Actions;

internal class ConsoleWriteAction : IAction
{
    public string Name { get; set; }

    public void Execute()
    {
        Console.WriteLine(Name);
    }
}
