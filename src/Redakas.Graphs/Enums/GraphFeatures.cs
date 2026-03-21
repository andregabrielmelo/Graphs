namespace Redakas.Graphs.Enums;

[Flags]
public enum GraphFeatures
{
    None = 0,
    Weighted = 1,
    Dynamic = 2, // TODO: Maybe this is not the best way to represent dynamic graphs, but for now it will do
}
