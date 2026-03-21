using Redakas.Graphs.Entities;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Redakas.Graphs.Helpers;

public class GraphHelpers
{
    // Method to show the graph matrix (vertices and their values)
    public static void ShowGraphMatrix(Graph graph)
    {
        // Check if the graph is null or doesn't have vertices
        if (graph == null || !graph.Vertices.Any())
        {
            Console.WriteLine("[red]O grafo não contém vértices para mostrar a matriz.[/]");
            return;
        }

        // Display the header row with vertices
        Console.WriteLine("\nMatriz de Adjacência:");
        Console.Write("           "); // Space for the row headers

        // Print the vertex names at the top (column headers)
        foreach (var vertex in graph.Vertices)
        {
            Console.Write($"{vertex.Name,10}"); // Adjust column width to 10
        }
        Console.WriteLine();

        // Print a line separator
        Console.WriteLine(new string('-', 10 * (graph.Vertices.Count + 1)));

        // Print each row for each vertex
        foreach (var fromVertex in graph.Vertices)
        {
            // Start with the row header (vertex name)
            Console.Write($"{fromVertex.Name,10}");

            // For each vertex, print "1" if there's an edge, else print "0"
            foreach (var toVertex in graph.Vertices)
            {
                bool hasEdge = graph.HasEdge(fromVertex, toVertex);

                // You can also print edge weights or other values if desired
                Console.Write($"{(hasEdge ? "1" : "0"),10}"); // Use "1" or "0" for edges/no edges
            }
            Console.WriteLine(); // Move to the next line after each row
        }
    }
}
