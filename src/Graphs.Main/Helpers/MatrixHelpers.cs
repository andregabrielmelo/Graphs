namespace Graphs.Main.Helpers;

internal class MatrixHelpers
{
    public static void ShowMatrix<T>(T[,] matrix)
    {
        int size = matrix.GetLength(0);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Console.Write($"{matrix[i, j]} ");
            }
            Console.WriteLine();
        }
    }
}
