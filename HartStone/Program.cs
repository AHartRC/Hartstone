using System;
using System.Collections.Generic;
using System.Linq;
using HartStone.Classes;
using HartStone.Models;

namespace HartStone
{
    public class Program
    {
        static HSEngine engine = new HSEngine();

        static void Main(string[] args)
        {
            Console.BufferHeight = Console.BufferHeight * 30;
            Console.BufferWidth = Console.BufferWidth * 3;

            Console.WriteLine("Welcome to Hartstone! It's ugly as sin, but full of win =-P\r\n");
            Console.WriteLine("You are in a dark place. You are likely to be eaten by a grue.\r\n");
            
            Console.WriteLine("Let's get started!");

            if (engine.Start())
            {
                engine.Run();
            }
            else
            {
                Console.WriteLine("The engine failed to start. Please check the output for any possible indication as to what went wrong.");
            }

            Console.WriteLine("\r\n\r\n\r\nThe application has finished running. Press any key to exit the application");
            Console.ReadKey(true);
        }
    }
}
