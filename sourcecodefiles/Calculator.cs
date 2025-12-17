
using System;
using System.Globalization;

public class Calculator
{
    // Entry point: provides a simple REPL calculator
    public static void Main(string[] args)
    {
        Console.WriteLine("Calculator (single file, single class: Calculator)");
        Console.WriteLine("Supported operators: +  -  *  /  %  ^");
        Console.WriteLine("Examples: 12 + 3    7.5 * 2    -8 / 2    10 % 3    2 ^ 10");
        Console.WriteLine("Type 'exit' to quit.\n");

        while (true)
        {
            Console.Write("calc> ");
            string? line = Console.ReadLine();

            if (line == null)
                break; // End of input

            line = line.Trim();
            if (line.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (line.Length == 0)
                continue;

            try
            {
                double result = EvaluateBinaryExpression(line);
                Console.WriteLine(result.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        Console.WriteLine("Goodbye!");
    }

    /// <summary>
    /// Evaluates a simple binary expression of the form:
    ///   <number> <operator> <number>
    /// Supported operators: +, -, *, /, %, ^
    /// </summary>
    private static double EvaluateBinaryExpression(string input)
    {
        // Expect exactly three tokens: lhs, op, rhs (e.g., "12.5 * 3")
        var tokens = input.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length != 3)
            throw new FormatException("Please enter: <number> <operator> <number>. Example: 12.5 * 3");

        double left = ParseDouble(tokens[0]);
        string op = tokens[1];
        double right = ParseDouble(tokens[2]);

        switch (op)
        {
            case "+": return left + right;
            case "-": return left - right;
            case "*": return left * right;
            case "/":
                if (right == 0.0)
                    throw new DivideByZeroException("Division by zero is not allowed.");
                return left / right;
            case "%":
                if (right == 0.0)
                    throw new DivideByZeroException("Modulo by zero is not allowed.");
                return left % right;
            case "^": return Math.Pow(left, right);
            default:
                throw new NotSupportedException($"Unsupported operator '{op}'. Use one of: +  -  *  /  %  ^");
        }
    }

    /// <summary>
    /// Parses a string into double using invariant culture (avoids locale issues).
    /// Accepts: 12, 12.5, -3.14, 1e3, -2.5e-2
    /// </summary>
    private static double ParseDouble(string s)
    {
        if (!double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands,
                             CultureInfo.InvariantCulture, out double value))
        {
            throw new FormatException($"Invalid number: '{s}'. Try formats like 12, -3.5, 1.2e3");
        }
               return value;
    }
