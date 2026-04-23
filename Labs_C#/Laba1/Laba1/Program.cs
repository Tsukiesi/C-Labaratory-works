using System;
using System.Collections.Generic;
using System.Text;

namespace Laba1
{

    // Задание 1: Вариант 6 — Матрица
    public class Matrix : IEquatable<Matrix>
    {
        private readonly double[,] _data;
        public int Rows { get; }
        public int Columns { get; }

        public Matrix(double[,] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Rows = data.GetLength(0);
            Columns = data.GetLength(1);
            _data = (double[,])data.Clone();
        }

        public double this[int row, int col] => _data[row, col];

        public double Determinant
        {
            get
            {
                if (Rows != Columns) throw new InvalidOperationException("Определитель существует только для квадратных матриц.");
                return CalculateDeterminant(_data);
            }
        }

        private double CalculateDeterminant(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n == 1) return matrix[0, 0];
            if (n == 2) return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
            double det = 0;
            for (int i = 0; i < n; i++)
                det += Math.Pow(-1, i) * matrix[0, i] * CalculateDeterminant(GetMinor(matrix, 0, i));
            return det;
        }

        private double[,] GetMinor(double[,] matrix, int row, int col)
        {
            int n = matrix.GetLength(0);
            double[,] minor = new double[n - 1, n - 1];
            for (int i = 0, mi = 0; i < n; i++)
            {
                if (i == row) continue;
                for (int j = 0, mj = 0; j < n; j++)
                {
                    if (j == col) continue;
                    minor[mi, mj] = matrix[i, j];
                    mj++;
                }
                mi++;
            }
            return minor;
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new ArgumentException("Для сложения размеры матриц должны совпадать.");

            double[,] res = new double[a.Rows, a.Columns];
            for (int i = 0; i < a.Rows; i++)
                for (int j = 0; j < a.Columns; j++)
                    res[i, j] = a[i, j] + b[i, j];
            return new Matrix(res);
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.Columns != b.Rows)
                throw new ArgumentException("Кол-во столбцов первой матрицы должно быть равно кол-ву строк второй.");

            double[,] res = new double[a.Rows, b.Columns];
            for (int i = 0; i < a.Rows; i++)
                for (int j = 0; j < b.Columns; j++)
                    for (int k = 0; k < a.Columns; k++)
                        res[i, j] += a[i, k] * b[k, j];
            return new Matrix(res);
        }

        public static Matrix operator ~(Matrix m)
        {
            double[,] res = new double[m.Columns, m.Rows];
            for (int i = 0; i < m.Rows; i++)
                for (int j = 0; j < m.Columns; j++)
                    res[j, i] = m[i, j];
            return new Matrix(res);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Rows; i++)
            {
                sb.Append("| ");
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append($"{_data[i, j]:F2} ");
                }
                sb.AppendLine("|");
            }
            return sb.ToString();
        }

        public bool Equals(Matrix other)
        {
            if (other == null || Rows != other.Rows || Columns != other.Columns) return false;
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    if (Math.Abs(_data[i, j] - other._data[i, j]) > 1e-9) return false;
            return true;
        }



        public override bool Equals(object obj) => Equals(obj as Matrix);
        public override int GetHashCode() => Determinant.GetHashCode();

        public static bool operator ==(Matrix a, Matrix b) => Equals(a, b);
        public static bool operator !=(Matrix a, Matrix b) => !Equals(a, b);
    }

    // Задание 2: Вариант 6 — Стратегия
    public interface IDiscountStrategy { decimal Calculate(decimal amount); }

    public class NoDiscount : IDiscountStrategy { public decimal Calculate(decimal a) => 0; }

    public class PercentageDiscount : IDiscountStrategy
    {
        private decimal _p; public PercentageDiscount(decimal p) => _p = p;
        public decimal Calculate(decimal a) => a * (_p / 100);
    }

    public class FixedDiscount : IDiscountStrategy
    {
        private decimal _val; public FixedDiscount(decimal v) => _val = v;
        public decimal Calculate(decimal a) => Math.Min(a, _val);
    }

    public class Order
    {
        private IDiscountStrategy _s = new NoDiscount();
        public decimal Amount { get; }
        public Order(decimal a) => Amount = a;
        public void SetStrategy(IDiscountStrategy s) => _s = s;
        public decimal GetFinalPrice() => Amount - _s.Calculate(Amount);
    }

    // Задание 3: Вариант 2 — Дерево
    public class TreeNode
    {
        public string Value { get; set; }
        public List<TreeNode> Children { get; } = new List<TreeNode>();
        public TreeNode(string v) => Value = v;
        public void Add(TreeNode child) => Children.Add(child);

        public TreeNode Find(string val)
        {
            if (Value.Equals(val, StringComparison.OrdinalIgnoreCase)) return this;
            foreach (var child in Children)
            {
                var f = child.Find(val);
                if (f != null) return f;
            }
            return null;
        }

        public void Print(int level = 0)
        {
            Console.WriteLine(new string(' ', level * 2) + "└─ " + Value);
            foreach (var child in Children) child.Print(level + 1);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Задание 1: Матрицы ===");

            Matrix m1 = new Matrix(new double[,] { { 1, 2 }, { 3, 4 } });
            Matrix m2 = new Matrix(new double[,] { { 5, 6 }, { 7, 8 } });

            Console.WriteLine("Матрица M1:");
            Console.WriteLine(m1);
            Console.WriteLine($"Определитель M1: {m1.Determinant}");

            Console.WriteLine("\nМатрица M2:");
            Console.WriteLine(m2);
            Console.WriteLine($"Определитель M2: {m2.Determinant}");

            Console.WriteLine("\nРезультат сложения (M1 + M2):");
            Console.WriteLine(m1 + m2);

            Console.WriteLine("Результат умножения (M1 * M2):");
            Console.WriteLine(m1 * m2);

            Console.WriteLine("Транспонированная матрица M1 (~M1):");
            Console.WriteLine(~m1);

            Console.WriteLine($"Проверка на равенство (M1 == M2): {m1 == m2}");

            Console.WriteLine("\n=== Задание 2: Тестирование стратегий скидок ===");

            decimal initialAmount = 5000m;
            Order myOrder = new Order(initialAmount);
            Console.WriteLine($"Исходная сумма заказа: {myOrder.Amount}");

            myOrder.SetStrategy(new NoDiscount());
            Console.WriteLine($"1. Без скидки: {myOrder.GetFinalPrice()}");

            decimal percent = 15;
            myOrder.SetStrategy(new PercentageDiscount(percent));
            Console.WriteLine($"2. Скидка {percent}%: {myOrder.GetFinalPrice()}");

            decimal fixedBonus = 1200m;
            myOrder.SetStrategy(new FixedDiscount(fixedBonus));
            Console.WriteLine($"3. Фиксированная скидка {fixedBonus}: {myOrder.GetFinalPrice()}");

            Order smallOrder = new Order(500m);
            smallOrder.SetStrategy(new FixedDiscount(1000m));
            Console.WriteLine($"4. Заказ {smallOrder.Amount}, скидка 1000: {smallOrder.GetFinalPrice()} (защита от отрицательной цены)");

            Console.WriteLine();

            Console.WriteLine("=== Задание 3: Дерево ===");
            TreeNode root = new TreeNode("Университет");
            TreeNode f1 = new TreeNode("ИТ-Факультет");
            f1.Add(new TreeNode("Кафедра ООП"));
            f1.Add(new TreeNode("Кафедра ИБ"));
            root.Add(f1);
            root.Add(new TreeNode("Эконом-Факультет"));

            root.Print();
            string search = "Кафедра ООП";
            Console.WriteLine($"\nПоиск '{search}': {(root.Find(search) != null ? "Найдено" : "Не найдено")}");

            string search1 = "Кафедра Математики";
            Console.WriteLine($"\nПоиск '{search1}': {(root.Find(search1) != null ? "Найдено" : "Не найдено")}");

            Console.ReadKey();
        }
    }
}