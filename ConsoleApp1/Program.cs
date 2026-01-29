using System;
using System.Text;

public class Matrix
{
    private double[,] data;

    // Размеры матрицы
    public int Rows { get; }
    public int Columns { get; }

    // Индексатор для доступа к элементам матрицы
    public double this[int row, int col]
    {
        get => data[row, col];
        set => data[row, col] = value;
    }

    // Конструктор с указанием размеров
    public Matrix(int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
            throw new ArgumentException("Размеры матрицы должны быть положительными числами");

        Rows = rows;
        Columns = cols;
        data = new double[rows, cols];
    }

    // Конструктор из двумерного массива
    public Matrix(double[,] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        Rows = array.GetLength(0);
        Columns = array.GetLength(1);
        data = (double[,])array.Clone();
    }

    // Заполнение матрицы случайными значениями
    public void FillRandom(double minValue = -10, double maxValue = 10)
    {
        Random rand = new Random();
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                data[i, j] = rand.NextDouble() * (maxValue - minValue) + minValue;
            }
        }
    }

    // Получение минора матрицы (для вычисления определителя)
    private Matrix GetMinor(int rowToRemove, int colToRemove)
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Минор можно получить только для квадратной матрицы");

        Matrix minor = new Matrix(Rows - 1, Columns - 1);

        int minorRow = 0;
        for (int i = 0; i < Rows; i++)
        {
            if (i == rowToRemove) continue;

            int minorCol = 0;
            for (int j = 0; j < Columns; j++)
            {
                if (j == colToRemove) continue;

                minor[minorRow, minorCol] = data[i, j];
                minorCol++;
            }
            minorRow++;
        }

        return minor;
    }

    // Вычисление определителя методом разложения по строке/столбцу
    public double Determinant()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Определитель можно вычислить только для квадратной матрицы");

        // Базовый случай: матрица 1x1
        if (Rows == 1)
            return data[0, 0];

        // Базовый случай: матрица 2x2
        if (Rows == 2)
            return data[0, 0] * data[1, 1] - data[0, 1] * data[1, 0];

        // Базовый случай: матрица 3x3 (правило Саррюса)
        if (Rows == 3)
        {
            return data[0, 0] * data[1, 1] * data[2, 2] +
                   data[0, 1] * data[1, 2] * data[2, 0] +
                   data[0, 2] * data[1, 0] * data[2, 1] -
                   data[0, 2] * data[1, 1] * data[2, 0] -
                   data[0, 1] * data[1, 0] * data[2, 2] -
                   data[0, 0] * data[1, 2] * data[2, 1];
        }

        // Рекурсивный случай: разложение по первой строке
        double determinant = 0;
        int sign = 1;

        for (int j = 0; j < Columns; j++)
        {
            Matrix minor = GetMinor(0, j);
            determinant += sign * data[0, j] * minor.Determinant();
            sign = -sign; // Чередование знаков
        }

        return determinant;
    }

    // Переопределение метода ToString для красивого вывода
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        // Находим максимальную длину элемента для форматирования
        int maxLength = 0;
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                int length = data[i, j].ToString("F2").Length;
                if (length > maxLength) maxLength = length;
            }
        }

        // Формируем строковое представление матрицы
        for (int i = 0; i < Rows; i++)
        {
            sb.Append("| ");
            for (int j = 0; j < Columns; j++)
            {
                // Форматируем число с 2 знаками после запятой
                string formattedValue = data[i, j].ToString($"F2").PadLeft(maxLength);
                sb.Append(formattedValue);

                if (j < Columns - 1)
                    sb.Append("  ");
            }
            sb.Append(" |\n");
        }

        return sb.ToString();
    }

    // Статические методы для создания специальных матриц

    // Единичная матрица
    public static Matrix Identity(int size)
    {
        Matrix identity = new Matrix(size, size);
        for (int i = 0; i < size; i++)
        {
            identity[i, i] = 1.0;
        }
        return identity;
    }

    // Нулевая матрица
    public static Matrix Zero(int rows, int cols)
    {
        return new Matrix(rows, cols);
    }
}

// Пример использования
class Program
{
    static void Main()
    {
        try
        {
            Console.WriteLine("=== Матрица 2x2 ===");
            Matrix m2 = new Matrix(2, 2);
            m2[0, 0] = 1; m2[0, 1] = 2;
            m2[1, 0] = 3; m2[1, 1] = 4;
            Console.WriteLine(m2);
            Console.WriteLine($"Определитель: {m2.Determinant():F2}\n");

            Console.WriteLine("=== Матрица 3x3 ===");
            Matrix m3 = new Matrix(3, 3);
            m3[0, 0] = 1; m3[0, 1] = 2; m3[0, 2] = 3;
            m3[1, 0] = 4; m3[1, 1] = 5; m3[1, 2] = 6;
            m3[2, 0] = 7; m3[2, 1] = 8; m3[2, 2] = 9;
            Console.WriteLine(m3);
            Console.WriteLine($"Определитель: {m3.Determinant():F2}\n");

            Console.WriteLine("=== Матрица 4x4 ===");
            double[,] array4x4 = {
                {1, 2, 3, 4},
                {5, 6, 7, 8},
                {9, 10, 11, 12},
                {13, 14, 15, 16}
            };
            Matrix m4 = new Matrix(array4x4);
            Console.WriteLine(m4);
            Console.WriteLine($"Определитель: {m4.Determinant():F2}\n");

            Console.WriteLine("=== Единичная матрица 3x3 ===");
            Matrix identity = Matrix.Identity(3);
            Console.WriteLine(identity);
            Console.WriteLine($"Определитель: {identity.Determinant():F2}\n");

            Console.WriteLine("=== Матрица со случайными значениями 3x3 ===");
            Matrix randomMatrix = new Matrix(3, 3);
            randomMatrix.FillRandom(-5, 5);
            Console.WriteLine(randomMatrix);
            Console.WriteLine($"Определитель: {randomMatrix.Determinant():F2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}