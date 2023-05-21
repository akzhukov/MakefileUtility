using MakefileUtility.Actions;
using MakefileUtility.Entities;

namespace MakefileUtility.Helpers;

internal static class MakefileParser
{
    public static Dictionary<string, Target> ParseFile(string filePath)
    {
        Dictionary<string, Target> targets = new();

        using (StreamReader reader = new(filePath))
        {
            string line;
            Target currentTarget = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (!line.StartsWith(' ') && !line.StartsWith('\t'))
                {
                    currentTarget = ParseTargetLine(line);

                    if (targets.ContainsKey(currentTarget.Name))
                        throw new InvalidDataException($"File contains targets with same name: {currentTarget.Name}");

                    targets.Add(currentTarget.Name, currentTarget);
                }
                else
                {
                    if (currentTarget is null)
                        throw new InvalidDataException("File must start with target name");

                    IAction action = ParseActionLine(line);
                    currentTarget.Actions.Add(action);
                }
            }
        }

        FillTargetDependencies(targets);

        return targets;
    }

    private static Target ParseTargetLine(string targetLine)
    {
        string[] lines = targetLine.Split(":");
        if (lines.Length > 2)
            throw new InvalidDataException("A line in a file cannot contain more than one colon");

        string[] dependencies = Array.Empty<string>();
        if (lines.Length == 2)
            dependencies = lines[1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        return new Target
        {
            Name = lines[0],
            DependencyNames = new HashSet<string>(dependencies)
        };
    }

    private static IAction ParseActionLine(string actionLine)
    {
        return new ConsoleWriteAction { Name = actionLine.Trim() };
    }

    private static void FillTargetDependencies(Dictionary<string, Target> targets)
    {
        foreach (var target in targets)
        {
            foreach (var dependencyName in target.Value.DependencyNames)
            {
                if (targets.TryGetValue(dependencyName, out var dependency))
                {
                    target.Value.Dependencies.Add(dependency);
                }
                else
                {
                    throw new InvalidDataException($"Target with name {dependencyName} does not exist!");
                }
            }
        }
    }
}
