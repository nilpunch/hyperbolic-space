using System.Collections.Generic;
using UnityEngine;

public interface IReduceRule
{
    string Reduce(string input);
}

public class ReduceRule : IReduceRule
{
    public readonly string From;
    public readonly string To;

    public ReduceRule(string from, string to)
    {
        From = from;
        To = to;
    }

    public string Reduce(string input)
    {
        if (From.Contains("u") == false)
        {
            if (input.Contains(From))
            {
                return input.Replace(From, To);
            }
            
            return input;
        }

        int depth = 0;
        while (true)
        {
            string newFromRule = "";
            string newToRule = "";
            
            foreach (var symbol in From)
            {
                newFromRule += symbol;

                if (symbol == 'u')
                {
                    for (int j = 0; j < depth; ++j)
                    {
                        newFromRule += 'u';
                    }
                }
            }

            if (input.Length < newFromRule.Length)
            {
                break;
            }

            foreach (var symbol in To)
            {
                newToRule += symbol;

                if (symbol == 'u')
                {
                    for (int j = 0; j < depth; ++j)
                    {
                        newToRule += 'u';
                    }
                }
            }

            depth += 1;
            
            if (input.Contains(newFromRule))
            {
                return input.Replace(newFromRule, newToRule);
            }
        }
        
        return input;
    }
}

public class FinishiserRule : IReduceRule
{
    public readonly string Postfix;

    public FinishiserRule(string postfix)
    {
        Postfix = postfix;
    }

    public string Reduce(string input)
    {
        if (input.Length < Postfix.Length)
            return input;

        if (Postfix.Contains("u") == false)
        {
            if (string.CompareOrdinal(input.Substring(input.Length - Postfix.Length, Postfix.Length), Postfix) == 0)
            {
                return input.Remove(input.Length - Postfix.Length, Postfix.Length);
            }

            return input;
        }
        
        int depth = 0;
        while (true)
        {
            string newPostfix = "";
            
            foreach (var symbol in Postfix)
            {
                newPostfix += symbol;

                if (symbol == 'u')
                {
                    for (int j = 0; j < depth; ++j)
                    {
                        newPostfix += 'u';
                    }
                }
            }

            if (input.Length < newPostfix.Length)
            {
                break;
            }

            depth += 1;
            
            if (string.CompareOrdinal(input.Substring(input.Length - newPostfix.Length, newPostfix.Length), newPostfix) == 0)
            {
                return input.Remove(input.Length - newPostfix.Length, newPostfix.Length);
            }
        }

        return input;
    }
}

public static class SquareCoordinateReducer
{
    private static readonly List<ReduceRule> _rules = new List<ReduceRule>();
    private static readonly List<FinishiserRule> _finishisers = new List<FinishiserRule>();

    static SquareCoordinateReducer()
    {
        _rules.Add(new ReduceRule("rl", ""));
        _rules.Add(new ReduceRule("lr", ""));
        _rules.Add(new ReduceRule("ll", "rr"));
        _rules.Add(new ReduceRule("rrr", "l"));
        _rules.Add(new ReduceRule("urru", "rr"));
        
        _rules.Add(new ReduceRule("uluulu", "luruurul"));
        _rules.Add(new ReduceRule("uruuruuru ", "ruluuulur"));
        
        _rules.Add(new ReduceRule("ururu", "rulur"));
        _rules.Add(new ReduceRule("ululu", "lurul"));


        // _finishisers.Add(new FinishiserRule("urru"));

        _finishisers.Add(new FinishiserRule("r"));
        _finishisers.Add(new FinishiserRule("rr"));
        _finishisers.Add(new FinishiserRule("l"));
        _finishisers.Add(new FinishiserRule("ll"));
    }

    public static string ReduceCoordinate(string input)
    {
        string output = ReduceCycle(input, _rules);
        output = ReduceCycle(output, _finishisers);

        return output;
    }

    private static string ReduceCycle(string input, IReadOnlyList<IReduceRule> reducers)
    {
        string output = (string) input.Clone();

        for (var index = 0; index < reducers.Count; index++)
        {
            string reducedOutput = reducers[index].Reduce(output);

            if (string.CompareOrdinal(output, reducedOutput) == 0)
                continue;

            output = reducedOutput;
            index = -1;
        }

        return output;
    }
}