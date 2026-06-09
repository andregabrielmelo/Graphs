namespace Redakas.Graphs.Entities;

// TODO: The right thing would be for each vertex to contain color instead of doing it this way, but this is easier to implement for now. Refactor later.
public record ColoringStep(Vertex Vertex, int Color, int StepNumber);
