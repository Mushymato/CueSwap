using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CueSwapGenerator;

internal record GenMethodModel(string Op, string TargetCls, string TargetMethod, int ArgOffset, string OldCue, string NewCue)
{
    internal string Op = Op;
    internal string TargetCls = TargetCls;
    internal string TargetMethod = TargetMethod;
    internal string TargetArgs = "";
    internal int ArgOffset = ArgOffset;
    internal string OldCue = OldCue;
    internal string NewCue = NewCue;
    internal string GeneratedMethodName = Regex.Replace($"T_{TargetCls}_{TargetMethod.Split(' ')[0]}_{OldCue}_{NewCue}", @"[./\\-]", "");

    internal string TargetOperand
    {
        get
        {
            StringBuilder sb = new("AccessTools.");
            sb.Append(Op switch
            {
                "Call" => "DeclaredMethod",
                "Callvirt" => "DeclaredMethod",
                "Stfld" => "DeclaredField",
                _ => throw new NotImplementedException(),
            });
            sb.Append("(typeof(");
            sb.Append(TargetCls);
            sb.Append("), \"");
            string[] methodParts = TargetMethod.Split(' ');
            sb.Append(methodParts[0]);
            sb.Append("\"");
            if (methodParts.Length > 1)
            {
                sb.Append(", [");
                for (int i = 1; i < methodParts.Length; i++)
                {
                    sb.Append("typeof(");
                    sb.Append(methodParts[i]);
                    if (i == methodParts.Length - 1)
                        sb.Append(")");
                    else
                        sb.Append("), ");
                }
                sb.Append("]");
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}

internal record GenClassModel(string Namespace, string ClassName, List<GenMethodModel> MethodModels, bool Debug)
{
    internal string Namespace = Namespace;
    internal string ClassName = ClassName;
    internal List<GenMethodModel> MethodModels = MethodModels;
    internal bool Debug = Debug;
};


[Generator]
public class CueSwapTranspilerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "CueSwapTranspilerAttribute.g.cs",
            SourceText.From(@"namespace CueSwapGenerator;

#pragma warning disable CS9113 // Parameter is unread.
[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class CueSwapTranspilerAttribute(string Op, string TargetCls, string TargetMethod, int ArgOffset, string OldCue, string NewCue) : System.Attribute { }
#pragma warning restore CS9113 // Parameter is unread.
",
            Encoding.UTF8)
        )
        );

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            "CueSwapGenerator.CueSwapTranspilerAttribute",
            (SyntaxNode node, CancellationToken cancellation) => node is ClassDeclarationSyntax,
            static (GeneratorAttributeSyntaxContext context, CancellationToken cancellation) =>
            {
                var containingClass = context.TargetSymbol.ContainingNamespace;
                bool debug = false;
                List<GenMethodModel> MethodModels = [];
                foreach (var attr in context.Attributes)
                {
                    if (attr.AttributeClass?.Name == "CueSwapTranspilerEnableLoggingAttribute")
                    {
                        debug = true;
                    }
                    else if (attr.AttributeClass?.Name == "CueSwapTranspilerAttribute")
                    {
                        MethodModels.Add(new(
                            (string)attr.ConstructorArguments[0].Value!,
                            (string)attr.ConstructorArguments[1].Value!,
                            (string)attr.ConstructorArguments[2].Value!,
                            (int)attr.ConstructorArguments[3].Value!,
                            (string)attr.ConstructorArguments[4].Value!,
                            (string)attr.ConstructorArguments[5].Value!
                        ));
                    }
                }
                return new GenClassModel(
                    Namespace: context.TargetSymbol.ContainingNamespace.Name ?? "GenerateCueSwap",
                    ClassName: context.TargetSymbol.Name,
                    MethodModels: MethodModels,
                    Debug: debug
                );
            }
        );

        context.RegisterSourceOutput(pipeline, static (context, clsModel) =>
        {
            StringBuilder srcBuilder = new(
$$"""
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace {{clsModel.Namespace}};

internal static partial class {{clsModel.ClassName}}
{
    internal static string CheckNewCueExists(string oldCue, string newCue)
    {
#if DEBUG
        ModEntry.Log($"{oldCue}->{newCue} ({Game1.soundBank.Exists(newCue)})");
#endif
        if (Game1.soundBank.Exists(newCue))
            return newCue;
        return oldCue;
    }
""");
            foreach (var model in clsModel.MethodModels)
            {
                srcBuilder.Append(
$$"""
    internal static IEnumerable<CodeInstruction> {{model.GeneratedMethodName}}(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        try
        {
            CodeMatcher matcher = new(instructions, generator);
            int count = 0;
            matcher
                .Start()
                .MatchStartForward([new(OpCodes.Ldstr, "{{model.OldCue}}"),
""");
                for (int i = 0; i < model.ArgOffset; i++)
                    srcBuilder.Append(" new(),");
                srcBuilder.Append(
$$"""
 new(OpCodes.{{model.Op}}, {{model.TargetOperand}})])
                .Repeat(
                    matchAction: (rmatch) =>
                    {
                        count++;
                        rmatch.Advance(1).InsertAndAdvance([
                            new(OpCodes.Ldstr, "{{model.NewCue}}"),
                            new(OpCodes.Call, AccessTools.DeclaredMethod(typeof({{clsModel.ClassName}}), nameof(CheckNewCueExists))),
                        ]);
                    }
                );
            if (count == 0)
                ModEntry.Log("Did not find transpile target ({{model.GeneratedMethodName}})", LogLevel.Warn);
            return matcher.Instructions();
        }
        catch (Exception err)
        {
            ModEntry.Log(
                $"Error in {{model.GeneratedMethodName}}:\n{err}",
                LogLevel.Error
            );
            return instructions;
        }
    }

"""
    );
            }
            srcBuilder.Append("}\n");

            var sourceText = SourceText.From(srcBuilder.ToString(), Encoding.UTF8);
            context.AddSource($"{clsModel.ClassName}.g.cs", sourceText);
        });
    }
}
