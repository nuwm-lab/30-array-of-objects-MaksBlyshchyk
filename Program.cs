using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ConvexQuadrilateralsApp
{
    public class Point
    {
        public double X { get; }
        public double Y { get; }
        public Point(double x, double y) { X = x; Y = y; }

        public double DistanceTo(Point p)
        {
            double dx = X - p.X;
            double dy = Y - p.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString() => $"({X.ToString("G", CultureInfo.InvariantCulture)}, {Y.ToString("G", CultureInfo.InvariantCulture)})";
    }

    public class ConvexQuadrilateral
    {
        // vertices ordered counter-clockwise,
        public IReadOnlyList<Point> Vertices { get; }

        public ConvexQuadrilateral(IEnumerable<Point> vertices)
        {
            var v = vertices.ToList();
            if (v.Count != 4) throw new ArgumentException("Потрібно 4 вершини.");
            Vertices = v;
        }

        public double Perimeter()
        {
            double p = 0;
            for (int i = 0; i < Vertices.Count; i++)
            {
                var a = Vertices[i];
                var b = Vertices[(i + 1) % Vertices.Count];
                p += a.DistanceTo(b);
            }
            return p;
        }

        public override string ToString()
        {
            return string.Join(" ", Vertices.Select((pt, i) => $"{(char)('A' + i)}{pt}"));
        }
    }

    class Program
    {
        const double EPS = 1e-9;

        static void Main()
        {
            Console.WriteLine("Програма: знайти опуклий чотирикутник з найбільшим периметром.");
            int n = ReadPositiveInt("Введіть кількість чотирикутників n: ");

            var quadrilaterals = new List<ConvexQuadrilateral>();

            for (int i = 0; i < n; i++)
            {
                Console.WriteLine($"\nЧотирикутник #{i + 1}:");
                while (true)
                {
                    var pts = new List<Point>();
                    bool failedInput = false;

                    for (int v = 0; v < 4; v++)
                    {
                        Console.Write($"Точка {(char)('A' + v)} (формат: x y): ");
                        string line = Console.ReadLine();
                        if (!TryParsePoint(line, out Point p))
                        {
                            Console.WriteLine("Невірний формат точки. Введіть чотирикутник ще раз з початку.");
                            failedInput = true;
                            break;
                        }
                        pts.Add(p);
                    }

                    if (failedInput) continue;

                    // Перевіримо дублікати вершин
                    if (HasDuplicatePoints(pts))
                    {
                        Console.WriteLine("Деякі вершини співпадають. Введіть інші координати.");
                        continue;
                    }

                    // Побудуємо випуклу оболонку з 4 точок (щоб врахувати порядок)
                    var hull = ConvexHull(pts);
                    if (hull.Count != 4)
                    {
                        Console.WriteLine("Ці точки не утворюють опуклий чотирикутник (колінеарні або ребра перетинаються). Спробуйте ще раз.");
                        continue;
                    }

                    // Перевіримо ненульову площу (оброблено у hull.Count != 4, але додатково чітко)
                    double area = PolygonArea(hull);
                    if (Math.Abs(area) < EPS)
                    {
                        Console.WriteLine("Площа дуже мала (вироджений полігон). Спробуйте інші точки.");
                        continue;
                    }

                    // Все добре — додамо квад
                    quadrilaterals.Add(new ConvexQuadrilateral(hull));
                    break;
                }
            }

            if (quadrilaterals.Count == 0)
            {
                Console.WriteLine("Жодного валідного чотирикутника не введено.");
                return;
            }

            // Знаходження максимального периметра
            double maxP = double.MinValue;
            int idx = -1;
            for (int i = 0; i < quadrilaterals.Count; i++)
            {
                double p = quadrilaterals[i].Perimeter();
                Console.WriteLine($"Периметр чотирикутника #{i + 1} = {p:F4}");
                if (p > maxP)
                {
                    maxP = p;
                    idx = i;
                }
            }

            Console.WriteLine("\nЧотирикутник з найбільшим периметром:");
            Console.WriteLine($"№{idx + 1}: {quadrilaterals[idx]}");
            Console.WriteLine($"Максимальний периметр = {maxP:F4}");
        }

        static int ReadPositiveInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = Console.ReadLine();
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) && n > 0)
                    return n;
                Console.WriteLine("Невірне ціле число (>0). Спробуйте ще раз.");
            }
        }

        static bool TryParsePoint(string line, out Point p)
        {
            p = null;
            if (string.IsNullOrWhiteSpace(line)) return false;
            var parts = line.Trim()
                            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;
            if (!double.TryParse(parts[0], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double x))
                return false;
            if (!double.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double y))
                return false;
            p = new Point(x, y);
            return true;
        }

        static bool HasDuplicatePoints(List<Point> pts)
        {
            for (int i = 0; i < pts.Count; i++)
                for (int j = i + 1; j < pts.Count; j++)
                    if (Math.Abs(pts[i].X - pts[j].X) < EPS && Math.Abs(pts[i].Y - pts[j].Y) < EPS)
                        return true;
            return false;
        }

        // Andrew's monotone chain convex hull (returns CCW, no duplicate of first at end).
        static List<Point> ConvexHull(List<Point> pts)
        {
            var points = pts.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            if (points.Count <= 1) return new List<Point>(points);

            List<Point> lower = new List<Point>();
            foreach (var p in points)
            {
                while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= EPS)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            List<Point> upper = new List<Point>();
            for (int i = points.Count - 1; i >= 0; i--)
            {
                var p = points[i];
                while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= EPS)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            // З'єднати, але уникнути повторення крайніх точок
            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            var hull = new List<Point>();
            hull.AddRange(lower);
            hull.AddRange(upper);

            // Якщо всі точки колінеарні — hull може містити менше точок
            return hull;
        }

        // векторний добуток (O->A) x (O->B)
        static double Cross(Point O, Point A, Point B)
        {
            return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        }

        // площа полігону через формулу Гауса (може бути додатною/від'ємною залежно від орієнтації)
        static double PolygonArea(IList<Point> pts)
        {
            double s = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                var a = pts[i];
                var b = pts[(i + 1) % pts.Count];
                s += a.X * b.Y - a.Y * b.X;
            }
            return Math.Abs(s) * 0.5;
        }
    }
}
