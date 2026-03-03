using System;
using System.Reflection;
using Azure.AI.Projects;

class Program
{
    static void Main()
    {
        var type = typeof(AIProjectClient);
        foreach (var method in type.GetMethods())
        {
            Console.WriteLine(method.Name);
        }
    }
}
