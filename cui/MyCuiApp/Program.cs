using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, CUI World!");

        Console.Write("Your name? > ");
        string name = Console.ReadLine();

        Console.WriteLine($"Mr.{name} .");
    }
}
