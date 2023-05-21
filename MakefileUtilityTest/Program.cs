using MakefileUtility.TargetExecutor;

if (args.Length == 0)
{
    Console.WriteLine("The name of the executable task is not set");
    return;
}
if (args.Length > 1)
{
    Console.WriteLine("Only one target can be given to execute");
    return;
}

var file = @$"makefile.txt";

try
{
    ITargetExecutor executor = new TargetExecutor(file);
    executor.Execute(args[0]);
}
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
}
