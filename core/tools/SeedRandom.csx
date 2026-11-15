int seed = 42;
Random rnd = new Random(seed);

Console.WriteLine(rnd.Next(1, 101)); // 67 (you kidding me?)
Console.WriteLine(rnd.Next(1, 101)); // 14
Console.WriteLine(rnd.Next(1, 101)); // 13