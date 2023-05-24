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

        ExecuteTargetWithDependencies(target);
    }

    private void ExecuteTargetWithDependencies(Target executingTarget)
    {
        var orderedTargets = GetOrderedTargetDependecies(executingTarget);

        foreach (var target in orderedTargets)
        {
            foreach (var action in target.Actions)
            {
                action.Execute();
            }
        }
    }

    private record IndexedTarget(Target Target, int Index);
    private IEnumerable<Target> GetOrderedTargetDependecies(Target executingTarget)
    {
        List<Target> result = new();
        Stack<IndexedTarget> indexedTargetStack = new();

        IndexedTarget lastVisitedTarget = new(executingTarget, 0);
        Target emptyTarget = new() { Name = string.Empty};
        int dependencyIndex = 0;

        while (indexedTargetStack.Count > 0 || executingTarget is not null)
        {
            if (executingTarget is not null)
            {
                CheckExecutingTargetState();
                MoveToDeeperLevel(new IndexedTarget(executingTarget, dependencyIndex));
            }
            else
            {
                IndexedTarget peekedTarget = indexedTargetStack.Peek();
                dependencyIndex = peekedTarget.Index;

                if (!MoveToNextDependency(peekedTarget))
                {
                    ExecuteTarget(peekedTarget);
                    lastVisitedTarget = peekedTarget;
                }
            }
        }

        return result;

        bool MoveToNextDependency(IndexedTarget target)
        {
            if (target.Target.Dependencies.Count > 1
                && lastVisitedTarget.Index != target.Target.Dependencies.Count - 1)
            {
                dependencyIndex = lastVisitedTarget.Index + 1;
                executingTarget = target.Target.Dependencies[dependencyIndex];
                return true;
            }

            return false;
        }

        void MoveToDeeperLevel(IndexedTarget target)
        {
            indexedTargetStack.Push(target);
            dependencyIndex = 0;
            executingTarget = target.Target.Dependencies.Any() ? target.Target.Dependencies[0] : null;
        }

        void ExecuteTarget(IndexedTarget target)
        {
            result.Add(target.Target);
            indexedTargetStack.Pop();
            _targetStates[target.Target] = TargetState.Executed;
        }

        void CheckExecutingTargetState()
        {
            if (!_targetStates.TryGetValue(executingTarget, out var state))
            {
                _targetStates.Add(executingTarget, TargetState.PrepareToExecution);
                return;
            }

            //данная задача уже есть в цепочке зависимостей,
            //значит ориентированный граф содержит цикл и выполнение задачи невозможно
            if (state == TargetState.PrepareToExecution)
                throw new InvalidDataException($"Unable to execute target. Dependencies graph contains cycle.");

            //если задача уже выполнялась, то повторно ее и ее зависимости выполнять не надо
            //поэтому подменяем ее на пустую задачу без зависимостей
            if (state == TargetState.Executed)
                executingTarget = emptyTarget;
        }
    }
}

