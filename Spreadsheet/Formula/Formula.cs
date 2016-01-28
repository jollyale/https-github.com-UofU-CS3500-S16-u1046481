// Skeleton written by Joe Zachary for CS 3500, January 2015
// Revised by Joe Zachary, January 2016
// JLZ Repaired pair of mistakes, January 23, 2016

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
/// <summary>
/// 
/// </summary>
namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public class Formula
    {
        /// <summary>
        /// This is the list of tokens with no spaces that will be built by the constructor and used by other methods.
        /// </summary>
        private List<string> tokens;
        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        public Formula(String formula)
        {
            // It checks if there is anything in the inserted formula.
            if (formula == null)
            {
                throw new FormulaFormatException("Nothing to calculate");
            }
            // It takes the white spaces out of the formula.
            string formula1 = formula.Trim();
            // It builds form (list outside of the constructor).
            tokens = new List<string>(GetTokens(formula1));
            // It checks if there is at least one token.
            if (tokens.Count() < 1)
            {
                throw new FormulaFormatException("Nothing to calculate");
            }
            double result;
            int lpar = 0, rpar = 0;
            // It checks if the first token is a valid entry as a first token.
            if (double.TryParse(tokens[0].Trim(), out result) == false && (Regex.IsMatch(tokens[0].Trim(), "^[A-Za-z]{1}?[0-9]*$") == false) && "(".Contains(tokens[0].Trim()) == false)
            {
                throw new FormulaFormatException("Only numbers, variables, or a left parenthesis are allowed as the first character.");
        }
            if (tokens[0] == "(")
            {
                lpar += 1;
            }
            // It checks that the last token is a number, variable or closing parenthesis.
            if (double.TryParse(tokens[tokens.Count() - 1], out result) == false && (Regex.IsMatch(tokens[tokens.Count() - 1], "^[A-Za-z]{1}?[0-9]*$") == false) && tokens[tokens.Count() - 1] != ")")
            {
                throw new FormulaFormatException("Only numbers, variables, or a left parenthesis are allowed as the first character.");
            }
            for (int i = 1; i <= (tokens.Count() - 1); i++)
            {
                string s = tokens[i].Trim(), t = tokens[i - 1].Trim();
                bool ntf = double.TryParse(s, out result), vtf = Regex.IsMatch(s, "^[A-Za-z]{1}?[0-9]*$");
                bool tntf = double.TryParse(t, out result), tvtf = Regex.IsMatch(t, "^[A-Za-z]{1}?[0-9]*$");

                // It checks if there are any invalid tokens.
                if ((ntf == false && vtf == false) && ("()+-*/".Contains(s) == false))
                {
                    throw new FormulaFormatException("Invalid character in the expression.");
                }
                // It checks that the valid tokens are in the right order.
                if (tntf || tvtf || ")".Contains(t))
                {
                    if ("+-*/)".Contains(s) == false)
                    {
                        throw new FormulaFormatException("Invalid entry after variable, number or closing parenthesis");
                    }
                }
                else if (t == "(" || "+-*/".Contains(t))
                {
                    if (ntf == false && vtf == false && "(".Contains(s) == false)
                    {
                        throw new FormulaFormatException("Invalid entry after operator or opening parenthesis");
                    }
                }

                // It checks that at any point the number of left parenthesis in never greater than the number of right parenthesis.
                if (s == "(")
                {
                    lpar += 1;
                }
                if (s == ")")
                {
                    rpar += 1;
                    if (rpar > lpar)
                    {
                        throw new FormulaFormatException("Missing left parethesis.");
                    }
                }
            }
            // It checks that, in the formula, the number of left and right parenthesis is equal.
            if (lpar != rpar)
            {
                throw new FormulaFormatException("Missing parenthesis.");
            }
        }
        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)
        {
            // These create two stacks, on for the values and one for the operators.
            Stack<double> values = new Stack<double>();
            Stack<string> operators = new Stack<string>();

            // This loop looks every token and evaluates the formula.
            foreach (string t in tokens)
            {
                // These are the variable in wich the values are stored step by step.
                double number, number2, result;

                // These two variables return booleans that tell if the token is a variable or a number.
                // tnum = token is number?
                // tvar = token is a variable?
                bool tnum = double.TryParse(t, out number), tvar = Regex.IsMatch(t, "^[A-Za-z]+$|^[A-Za-z]+[0-9]*$");

                // These return the number of elements in the values stack (valCount) and in the operators stach (opCount).
                int valCount = values.Count, opCount = operators.Count;

                // This is what happens when the first number or variable is found.
                if (valCount == 0 && (tnum || tvar))
                {
                    // If it is a number, it is simply pushed in the values stack.
                    if (tnum)
                    {
                        values.Push(number);
                    }
                    /*If it is a variable, it is converted into a number and pushed in the values stack.
                    If it is not convertible, an exeption is thrown.
                    */
                    else if (tvar)
                    {
                        try
                        {
                            number = lookup(t);
                            values.Push(number);
                        }
                        catch
                        {
                            throw new FormulaEvaluationException("Undifined variable");
                        }
                    }
                }

                // This is what happens when the first operator is found. It is simply pushed in the stack.
                else if (opCount == 0 && "+-*/(".Contains(t))
                {
                    operators.Push(t);
                }

                // This is the option for all the tokens that are not the first of their kind.
                else
                {
                    // This takes track of the top element in the operators stack.
                    string peek = operators.Peek();

                    // If the token is a number...
                    if (tnum)
                    {
                        /* If a "/" or a "*" is on top of the operators list, the token and the top number
                        in the value stack are applied to the operator.*/
                        if ("/".Contains(peek))
                        {
                            if (number == 0)
                            {
                                throw new FormulaEvaluationException("Division by zero");
                            }
                            result = values.Pop() / number;
                            values.Push(result);
                            operators.Pop();
                        }
                        else if ("*".Contains(peek))
                        {
                            result = values.Pop() * number;
                            values.Push(result);
                            operators.Pop();
                        }

                        // Otherwise, the value is pushed in the values stack.
                        else
                        {
                            values.Push(number);
                        }
                    }// If the token is a variable...
                    else if (tvar)
                    {
                        // The token is converted into a value.
                        number = lookup(t);

                        /* If a "/" or a "*" is on top of the operators list, the token and the top number
                        in the value stack are applied to the operator.*/
                        if ("/".Contains(peek))
                        {
                            if (number == 0)
                            {
                                throw new FormulaEvaluationException("Division by zero");
                            }
                            result = values.Pop() / number;
                            values.Push(result);
                            operators.Pop();
                        }
                        else if ("*".Contains(operators.Peek()))
                        {
                            result = values.Pop() * number;
                            values.Push(result);
                            operators.Pop();
                        }
                        else
                        {
                            // Otherwise, the value is pushed in the values stack.
                            values.Push(number);
                        }
                    }

                    /*If the token is an operator and peek is a "+" or a "-", the top operator
                    in the stack is applied to the two top number in the values stack.
                    The result is pushed in the values stack. The token is pushed in its stack as well.*/
                    else if (t == "+" || t == "-")
                    {
                        if (operators.Peek() == "+")
                        {
                            number = values.Pop();
                            number2 = values.Pop();
                            number = number2 + number;
                            values.Push(number);
                            operators.Pop();
                        }
                        else if (operators.Peek() == "-")
                        {
                            number = values.Pop();
                            number2 = values.Pop();
                            number = number2 - number;
                            values.Push(number);
                            operators.Pop();
                        }
                        operators.Push(t);
                    }

                    // If the token is a "*", "/" or "(", it is simply pushed into the operator stack.
                    else if (t == "*" || t == "/" || t == "(")
                    {
                        operators.Push(t);
                    }

                    // If the token is a ")"...
                    else if (t == ")")
                    {
                        /*If peek is a "+" or a "-", peek is applied to the two first values in the stack and popped.
                        The result is pushed in the values stack.
                        The left parenthesis is also popped*/
                        if (operators.Peek() == "+")
                        {
                            number = values.Pop();
                            number2 = values.Pop();
                            operators.Pop();
                            number = number + number2;
                            values.Push(number);
                        }
                        else if (operators.Peek() == "-")
                        {
                            number = values.Pop();
                            number2 = values.Pop();
                            operators.Pop();
                            number = number2 - number;
                            values.Push(number);
                        }

                        /*If peek is a "*" or a "/", peek is applied to the two first values in the stack and popped.
                        The result is pushed in the values stack.*/
                        operators.Pop();
                        if (operators.Count > 0)
                        {
                            if (operators.Peek() == "*")
                            {
                                number = values.Pop();
                                number2 = values.Pop();
                                number = number * number2;
                                values.Push(number);
                                operators.Pop();
                            }
                            else if (operators.Peek() == "/")
                            {
                                number = values.Pop();
                                number2 = values.Pop();
                                number = number2 / number;
                                values.Push(number);
                                operators.Pop();
                            }
                        }
                    }
                }
            }

            /*When all the tokens have been processed, if there are no more operators in the stack,
            the result is returned.*/
            if (operators.Count == 0)
            {
                double result = values.Pop();
                Console.Write(result);
                return result;
            }
            /* Otherwise the last operator in the stack is applied to the two last numbers in the stack
            and the result is returned.*/
            else
            {
                double number1, number2, result;
                number1 = values.Pop();
                number2 = values.Pop();
                if (operators.Peek() == "+")
                {
                    result = number1 + number2;
                    Console.Write(result);
                    return result;
                }
                else if (operators.Peek() == "-")
                {
                    result = number2 - number1;
                    Console.Write(result);
                    return result;
                }
            }
            return 0;
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string s);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }
}
