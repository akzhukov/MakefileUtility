namespace MakefileUtility.Actions;

internal interface IAction
{
    string Name { get; set; }

    void Execute();
}
