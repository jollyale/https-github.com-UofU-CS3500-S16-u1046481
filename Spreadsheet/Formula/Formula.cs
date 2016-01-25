// Skeleton written by Joe Zachary for CS 3500, January 2015
// Revised by Joe Zachary, January 2016

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        private List<string> form;
        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by one or more letters and/or digits), left and right
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
            // It checks if there is at least one token.
            if (GetTokens(formula).Count() == 0)
            {
                new FormulaFormatException("Nothing to calculate");
                return;
            }
            double result;
            // It checks if the first token is a valid entry as a first token.
            if (double.TryParse(GetTokens(formula).First(), out result) == false)
            {
                foreach (char c in GetTokens(formula).First())
                {
                    if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789(".Contains(c) == false)
                    {
                        new FormulaFormatException("Only numbers, variables, or a left parenthesis are allowed as the first character.");
                        return;
                    }
                }
            }
            int lpar = 0, rpar = 0;
            foreach (string s in GetTokens(formula))
            {
                // It checks if there are any invalid tokens.
                if (double.TryParse(s, out result) == false)
                {
                    foreach (char c in s)
                    {
                        if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+-*/()".Contains(c) == false)
                        {
                            new FormulaFormatException("Invalid character in the expression.");
                            return;
                        }

                    }
                }
                // It checks that the valid tokens are in the right order.
                // Example: 
                bool operatorlpar = true;
                bool numberrpar = true;
                if ("xyzXYZ1234567890+-*/().".Contains(s) == false)
                {
                    new FormulaFormatException("Invalid character in the expression.");
                    return;
                }
                if ("xyzXYZ1234567890(".Contains(s))
                {
                    if (operatorlpar == false)
                    {
                        new FormulaFormatException("Not valid operation");
                        return;
                    }
                    numberrpar = true;
                    operatorlpar = false;
                }
                else if ("+-*/".Contains(s))
                {
                    if (numberrpar == false)
                    {
                        new FormulaFormatException("Not valid operation");
                        return;
                    }
                    numberrpar = false;
                    operatorlpar = true;
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
                        new FormulaFormatException("Missing left parethesis.");
                        return;
                    }
                }
            }
            // It checks that the last token is a number, variable or clsing parenthesis.
            if (double.TryParse(GetTokens(formula).Last(), out result) == false)
            {
                foreach (char c in GetTokens(formula).Last())
                {
                    if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+-*/()".Contains(c) == false)
                    {
                        new FormulaFormatException("Only numbers, variables, or a right parenthesis are allowed as first character.");
                        return;
                    }
                }
            }
            // It checks that, in the formula, the number of left and right parenthesis is equal.
            if (lpar != rpar)
            {
                new FormulaFormatException("Missing parenthesis.");
                return;
            }
            // It creates a List of tokens (strings)
            foreach (string s in GetTokens(formula))
            {
                form.Add(s);
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
            Stack<double> values = new Stack<double>();
            Stack<string> operators = new Stack<string>();

            foreach(string t in form)
            {
                double number;
                double result;
                if (double.TryParse(t, out number))
                {
                    if ("/".Contains(operators.Peek()))
                    {
                        if (number == 0)
                        {
                            new UndefinedVariableException("Division by zero");
                            return 0;
                        }
                        number = values.Pop() / number;
                        values.Push(number);
                        operators.Pop();
                    }
                    if ("*".Contains(operators.Peek()))
                    {
                        number = values.Pop() * number;
                        values.Push(number);
                        operators.Pop();
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// one or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]+";
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
