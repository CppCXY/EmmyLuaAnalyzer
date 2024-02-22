using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.DataFlow;

public class TypeTighteningAnalyzer(LuaCompilation compilation) : FlowAnalyzerBase(compilation)
{
    public override void Analyze(ControlFlowGraph cfg, LuaSyntaxTree tree)
    {
        var variableTypes = new Dictionary<CfgNode, Dictionary<string, HashSet<string>>>();
        foreach (var block in cfg.Nodes)
        {
            variableTypes[block] = new Dictionary<string, HashSet<string>>();
        }

        bool changed;
        do
        {
            changed = false;
            foreach (var block in cfg.Nodes)
            {
                foreach (var statement in block.Statements)
                {
                    // Here you need to implement the logic to extract the type information from the statement.
                    // For example, if the statement is an assignment, you can extract the type of the right-hand side expression and add it to the possible types of the left-hand side variable.
                }

                foreach (var successor in block.Successors)
                {
                    foreach (var variable in variableTypes[block].Keys)
                    {
                        if (!variableTypes[successor].ContainsKey(variable))
                        {
                            variableTypes[successor][variable] = new HashSet<string>();
                        }

                        foreach (var type in variableTypes[block][variable])
                        {
                            if (!variableTypes[successor][variable].Contains(type))
                            {
                                variableTypes[successor][variable].Add(type);
                                changed = true;
                            }
                        }
                    }
                }
            }
        } while (changed);

        // Step 3: Interpretation
        // Here you can do whatever you want with the information about the possible types of each variable at each block.
        // For example, you could print it out:
        foreach (var block in cfg.Nodes)
        {
            foreach (var variable in variableTypes[block].Keys)
            {
                // System.Console.WriteLine(
                //     $"Variable {variable} in block {block.Id} can be of types: {string.Join(", ", variableTypes[block][variable])}");
            }
        }
    }
}
