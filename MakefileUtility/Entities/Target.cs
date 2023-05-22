using MakefileUtility.Actions;
using System.Diagnostics;

namespace MakefileUtility.Entities;

[DebuggerDisplay("{Name}")]
internal class Target
{
    public string Name { get; set; }
    public List<IAction> Actions { get; set; } = new List<IAction>();
    public List<Target> Dependencies { get; set; } = new List<Target>();
    public HashSet<string> DependencyNames { get; set; } = new HashSet<string>();

    public override bool Equals(object obj)
    {
        if (obj is not Target target)
            return false;

        return target.Name == Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
