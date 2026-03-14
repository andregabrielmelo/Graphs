namespace Redakas.Graphs.Helpers;

public class MatrixHelpers
{
    public const string Infinity = "\u221E";

    /// <summary>
    /// Displays the contents of a square matrix to the console.
    /// </summary>
    /// <remarks>Each element is written to the console in row-major order. Only square matrices are
    /// supported; if the array is not square, the output may be incomplete or incorrect.</remarks>
    /// <typeparam name="T">The type of the elements contained in the matrix.</typeparam>
    /// <param name="matrix">A two-dimensional, square array containing the elements to display. The array must have equal length for both
    /// dimensions.</param>
    public static void ShowMatrix<T>(T[,] matrix)
    {
        int size = matrix.GetLength(0);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                T value = matrix[i, j];
                string? printValue = value?.ToString();

                // TODO: Change this to properly handle all numeric types, not just double. This is a quick fix to make the output more readable for cases where the matrix contains double values that may be infinity.
                // And to properly print the symbol
                // Special handling for double infinity
                if (value is double d)
                {
                    if (double.IsPositiveInfinity(d))
                        printValue = Infinity;
                    //Console.Write($"{Symbols.Infinity} " );
                    //Console.Write($"Inf ");
                    else if (double.IsNegativeInfinity(d))
                        printValue = $"-{Infinity}";
                    //Console.Write($"-{Symbols.Infinity} ");
                    //Console.Write($"-Inf " );
                }

                Console.Write($"{printValue} ");
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Returns a one-dimensional array containing the elements of the specified row from a two-dimensional matrix.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the matrix.</typeparam>
    /// <param name="matrix">The two-dimensional array from which to retrieve the row.</param>
    /// <param name="rowIndex">The zero-based index of the row to retrieve. Must be within the bounds of the matrix.</param>
    /// <returns>An array containing the elements of the specified row in the matrix.</returns>
    public static T[] GetMatrixRow<T>(T[,] matrix, int rowIndex)
    {
        int columns = matrix.GetLength(1); // number of columns
        T[] row = new T[columns];
        for (int j = 0; j < columns; j++)
        {
            row[j] = matrix[rowIndex, j];
        }
        return row;
    }

    /// <summary>
    /// Retrieves the elements from a specified column in a two-dimensional array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the matrix.</typeparam>
    /// <param name="matrix">The two-dimensional array from which to extract the column. Cannot be null.</param>
    /// <param name="columnIndex">The zero-based index of the column to retrieve. Must be within the bounds of the matrix columns.</param>
    /// <returns>An array containing the elements of the specified column. The array length matches the number of rows in the
    /// matrix.</returns>
    public static T[] GetMatrixColumn<T>(T[,] matrix, int columnIndex)
    {
        int rows = matrix.GetLength(0); // number of rows
        T[] column = new T[rows];
        for (int i = 0; i < rows; i++)
        {
            column[i] = matrix[i, columnIndex];
        }
        return column;
    }
}
