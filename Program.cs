using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace ConvexQuadrilaterals
{
    public struct Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(Point other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString()
        {
            return $"({X.ToString(CultureInfo.InvariantCulture)}, {Y.ToString(CultureInfo.InvariantCulture)})";
        }
    }

    public class ConvexQuadrilateral
    {
        private readonly Point[] _vertices;

        public Point[] Vertices => _vertices;
        public double Perimeter { get; }

        public ConvexQuadrilateral(Point[] vertices)
        {
            if (vertices == null || vertices.Length != 4)
                throw new ArgumentException("Потрібно рівно 4 вершини.");

            _vertices = OrderVerticesByAngle(vertices);

            if (!IsConvex())
                throw new ArgumentException("Вершини не утворюють опуклий чотирикутник або мають колінеарні точки.");

            Perimeter = ComputePerimeter();
        }

        private static Point[] OrderVerticesByAngle(Point[] pts)
        {
            double cx = pts.Average(p => p.X);
            double cy = pts.Average(p => p.Y);
            return pts.OrderBy(p => Math.Atan2(p.Y - cy, p.X - cx)).ToArray();
        }

        private double ComputePerimeter()
        {
            double sum = 0;
            for (int i = 0; i < 4; i++)
                sum += _vertices[i].DistanceTo(_vertices[(i + 1) % 4]);
            return sum;
        }

        private bool IsConvex()
        {
            double[] cross = new double[4];
            for (int i = 0; i < 4; i++)
            {
                Point a = _vertices[i];
                Point b = _vertices[(i + 1) % 4];
                Point c = _vertices[(i + 2) % 4];

                double abx = b.X - a.X;
                double aby = b.Y - a.Y;
                double bcx = c.X - b.X;
                double bcy = c.Y - b.Y;

                cross[i] = abx * bcy - aby * bcx;
            }

            bool hasPositive = cross.Any(v => v > 1e-9);
            bool hasNegative = cross.Any(v => v < -1e-9);
            bool hasZero = cross.Any(v => Math.Abs(v) <= 1e-9);

            if (hasZero) return false;
            return !(hasPositive && hasNegative);
        }

        public override string ToString()
        {
            return $"Vertices: {string.Join(", ", _vertices.Select(v => v.ToString()))}, Perimeter = {Perimeter:F3}";
        }
    }

    public static class Program
    {
        public static void Main()
        {
            RunManualTests();

            Console.WriteLine("\nВведіть n (кількість чотирикутників):");
            if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0)
            {
                Console.WriteLine("Помилка: потрібно ввести додатне ціле число.");
                return;
            }

            var quads = new List<ConvexQuadrilateral>();
            var culture = CultureInfo.InvariantCulture;

            for (int i = 0; i < n; i++)
            {
                Console.WriteLine($"\nЧотирикутник #{i + 1}: введіть 4 вершини (x y):");
                var points = new Point[4];

                for (int j = 0; j < 4; j++)
                {
                    Console.Write($"Вершина {j + 1}: ");
                    string? input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine("Помилка: порожній рядок.");
                        j--;
                        continue;
                    }

                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2 ||
                        !double.TryParse(parts[0], NumberStyles.Float, culture, out double x) ||
                        !double.TryParse(parts[1], NumberStyles.Float, culture, out double y))
                    {
                        Console.WriteLine("Помилка формату. Введіть два числа через пробіл.");
                        j--;
                        continue;
                    }

                    points[j] = new Point(x, y);
                }

                try
                {
                    var quad = new ConvexQuadrilateral(points);
                    quads.Add(quad);
                    Console.WriteLine("✅ Додано: " + quad);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("❌ " + ex.Message);
                }
            }

            if (quads.Count == 0)
            {
                Console.WriteLine("\nНе знайдено жодного валідного опуклого чотирикутника.");
                return;
            }

            var maxQuad = quads.OrderByDescending(q => q.Perimeter).First();
            Console.WriteLine($"\n🏆 Найбільший периметр: {maxQuad.Perimeter:F3}");
            Console.WriteLine(maxQuad);
        }

        private static void RunManualTests()
        {
            Console.WriteLine("=== Ручні тести ===");

            try
            {
                var q1 = new ConvexQuadrilateral(new[]
                {
                    new Point(0, 0),
                    new Point(2, 0),
                    new Point(2, 2),
                    new Point(0, 2)
                });
                Console.WriteLine("Test 1 (OK): " + q1);
            }
            catch (Exception e)
            {
                Console.WriteLine("Test 1 (FAIL): " + e.Message);
            }

            try
            {
                var q2 = new ConvexQuadrilateral(new[]
                {
                    new Point(0,0),
                    new Point(1,0),
                    new Point(2,0),
                    new Point(0,1)
                });
                Console.WriteLine("Test 2 (FAIL expected): " + q2);
            }
            catch (Exception e)
            {
                Console.WriteLine("Test 2 (OK, fail expected): " + e.Message);
            }

            try
            {
                var q3 = new ConvexQuadrilateral(new[]
                {
                    new Point(0,0),
                    new Point(1,0),
                    new Point(1,0),
                    new Point(0,1)
                });
                Console.WriteLine("Test 3 (FAIL expected): " + q3);
            }
            catch (Exception e)
            {
                Console.WriteLine("Test 3 (OK, fail expected): " + e.Message);
            }
        }
    }
}
