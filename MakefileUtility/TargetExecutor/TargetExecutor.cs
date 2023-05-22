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

    private void ExecuteTarget(Target executedTarget)
    {
        List<Target> result = new();
        Stack<(Target, int)> stack = new();
        (Target Target, int Index) lastVisited = (executedTarget, 0);
        Target fakeTarget = new() { Name = "FakeTarget" };
        int index = 0;

        while (stack.Count > 0 || executedTarget is not null)
        {
            if (executedTarget is not null)
            {
                if (!_targetStates.TryGetValue(executedTarget, out var state))
                    _targetStates.Add(executedTarget, TargetState.PrepareToExecution);

                if (state == TargetState.PrepareToExecution)
                    throw new InvalidDataException($"Unable to execute target. Dependencies graph contains cycle.");

                if (state == TargetState.Executed)
                    executedTarget = fakeTarget;

                stack.Push((executedTarget, index));
                index = 0;
                executedTarget = executedTarget.Dependencies.Any() ? executedTarget.Dependencies[0] : null;
            }
            else
            {
                (Target Target, int Index) peeked = stack.Peek();
                Target target = peeked.Target;
                index = peeked.Index;

                if (target.Dependencies.Count > 1 && lastVisited.Index != target.Dependencies.Count - 1)
                {
                    index = lastVisited.Index + 1;
                    executedTarget = target.Dependencies[index];
                }
                else
                {
                    result.Add(target);
                    lastVisited = (target, index);
                    stack.Pop();
                    _targetStates[target] = TargetState.Executed;
                }
            }
        }

        foreach (var target in result)
        {
            ExecuteTargetActions(target);
        }
    }

    private void ExecuteTargetActions(Target target)
    {
        foreach (var action in target.Actions)
        {
            action.Execute();
        }
    }
}

