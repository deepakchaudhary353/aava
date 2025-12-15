
using System;
using System.Globalization;

public class Program
{
    // Entry point
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Simple Calculator (one file, one class) ===");
        Console.WriteLine("Supported operators: +  -  *  /  %  ^ (power)");
        Console.WriteLine("Enter expressions like: 12.5 * 3  or  2 ^ 8");
        Console.WriteLine("Type 'exit' to quit.\n");

        // Use invariant culture for predictable decimal parsing (e.g., dot as decimal separator)
        var culture = CultureInfo.InvariantCulture;

        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine();

            if (input == null)
            {
                // End-of-stream (e.g., piped input ended), exit gracefully
                break;
            }

            input = input.Trim();
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            if (input.Length == 0)
            {
                continue; // Just press enter → prompt again
            }

            // Try to parse input of the form: <number> <operator> <number>
            // We allow extra spaces; split on whitespace.
            string[] parts = input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                Console.WriteLine("Invalid format. Use: <number> <operator> <number> (e.g., 10 / 2)");
                continue;
            }

            if (!double.TryParse(parts[0], NumberStyles.Float | NumberStyles.AllowThousands, culture, out double left))
            {
                Console.WriteLine("Invalid left operand: " + parts[0]);
                continue;
            }

            string op = parts[1];

            if (!double.TryParse(parts[2], NumberStyles.Float | NumberStyles.AllowThousands, culture, out double right))
            {
                Console.WriteLine("Invalid right operand: " + parts[2]);
                continue;
            }

            try
            {
                double result = Calculate(left, op, right);
                Console.WriteLine($"= {result.ToString(culture)}");
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("Error: Division by zero.");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
            }
        }
    }

    // Core calculation logic
    private static double Calculate(double left, string op, double right)
    {
        switch (op)
        {
            case "+":
                return left + right;
            case "-":
                return left - right;
            case "*":
            case "x":
            case "X":
                return left * right;
            case "/":
                if (right == 0.0) throw new DivideByZeroException();
                return left / right;
            case "%":
                if (right == 0.0) throw new DivideByZeroException();
                return left % right;
            case "^":
                // Power: left ^ right
                return Math.Pow(left, right);
            default:
                throw new ArgumentException($"Unsupported operator '{op}'. Use + - * / % ^");
        }
    }
}
