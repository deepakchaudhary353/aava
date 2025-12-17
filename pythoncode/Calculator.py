class Calculator:
    """
    Calculator provides basic arithmetic operations: add, subtract, multiply, divide.
    """
    def add(self, a: float, b: float) -> float:
        """Return the sum of a and b."""
        return a + b

    def subtract(self, a: float, b: float) -> float:
        """Return the difference of a and b."""
        return a - b

    def multiply(self, a: float, b: float) -> float:
        """Return the product of a and b."""
        return a * b

    def divide(self, a: float, b: float) -> float:
        """Return the quotient of a and b. Raises ZeroDivisionError if b == 0."""
        if b == 0:
            raise ZeroDivisionError("Division by zero is not allowed.")
        return a / b
