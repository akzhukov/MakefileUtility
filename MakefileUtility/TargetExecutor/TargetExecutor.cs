using MakefileUtility.Entities;
using MakefileUtility.Helpers;

namespace MakefileUtility.TargetExecutor;

public class TargetExecutor : ITargetExecutor
{
    private readonly Dictionary<string, Target> _targets;

    private readonly Dictionary<Target, TargetState> _targetStates = new();

    public TargetExecutor(string filePath)
    {
        _targets = MakefileParser.ParseFile(filePath);
    }

    public void Execute(string targetName)
    {
        _targetStates.Clear();

        if (!_targets.TryGetValue(targetName, out var target))
            throw new InvalidDataException($"Target with name {targetName} does not exist");

        ExecuteTarget(target);
    }

    private void ExecuteTarget(Target target)
    {
        foreach (var dependency in target.Dependencies)
        {
            if (!_targetStates.TryGetValue(dependency, out var state))
            {
                _targetStates.Add(dependency, TargetState.PrepareToExecution);
            }

            if (state == TargetState.Executed)
                break;

            if (state == TargetState.PrepareToExecution)
                throw new InvalidDataException($"Unable to execute target. Dependencies graph contains cycle.");

            ExecuteTarget(dependency);
        }

        foreach (var action in target.Actions)
        {
            action.Execute();
        }

        _targetStates[target] = TargetState.Executed;
    }
}
