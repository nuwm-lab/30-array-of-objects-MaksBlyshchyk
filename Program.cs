using System;

class Point
{
    public double X { get; }
    public double Y { get; }

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    // Відстань між двома точками
    public double DistanceTo(Point p)
    {
        double dx = X - p.X;
        double dy = Y - p.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

class ConvexQuadrilateral
{
    public Point A { get; }
    public Point B { get; }
    public Point C { get; }
    public Point D { get; }

    public ConvexQuadrilateral(Point a, Point b, Point c, Point d)
    {
        A = a;
        B = b;
        C = c;
        D = d;
    }

    // Обчислення периметра
    public double Perimeter()
    {
        return A.DistanceTo(B) + B.DistanceTo(C) + C.DistanceTo(D) + D.DistanceTo(A);
    }

    public void Print()
    {
        Console.WriteLine($"A({A.X}, {A.Y}), B({B.X}, {B.Y}), C({C.X}, {C.Y}), D({D.X}, {D.Y})");
    }
}

class Program
{
    static void Main()
    {
        Console.Write("Введіть кількість чотирикутників n: ");
        int n = int.Parse(Console.ReadLine());

        ConvexQuadrilateral[] quads = new ConvexQuadrilateral[n];

        // Введення координат.
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"\nВведіть координати для чотирикутника №{i + 1}:");

            Console.Write("A(x y): ");
            var a = Console.ReadLine().Split(' ');
            Console.Write("B(x y): ");
            var b = Console.ReadLine().Split(' ');
            Console.Write("C(x y): ");
            var c = Console.ReadLine().Split(' ');
            Console.Write("D(x y): ");
            var d = Console.ReadLine().Split(' ');

            quads[i] = new ConvexQuadrilateral(
                new Point(double.Parse(a[0]), double.Parse(a[1])),
                new Point(double.Parse(b[0]), double.Parse(b[1])),
                new Point(double.Parse(c[0]), double.Parse(c[1])),
                new Point(double.Parse(d[0]), double.Parse(d[1]))
            );
        }

        // Знаходження найбільшого периметра
        double maxPerimeter = 0;
        int maxIndex = 0;

        for (int i = 0; i < n; i++)
        {
            double p = quads[i].Perimeter();
            Console.WriteLine($"Периметр чотирикутника №{i + 1} = {p:F2}");

            if (p > maxPerimeter)
            {
                maxPerimeter = p;
                maxIndex = i;
            }
        }

        Console.WriteLine("\nЧотирикутник з найбільшим периметром:");
        quads[maxIndex].Print();
        Console.WriteLine($"Максимальний периметр = {maxPerimeter:F2}");
    }
}

