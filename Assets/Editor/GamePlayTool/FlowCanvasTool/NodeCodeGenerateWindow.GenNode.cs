using System;
using System.IO;
using System.Reflection;
using CodeBuild;
using Cysharp.Threading.Tasks;
using Design.Proxy.Utils;
using Everlasting.Extend;
using GameBase.Log;
using GameBase.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.VersionControl;

namespace GamePlayTool.Editor.FlowCanvasTool
{
    public partial class NodeCodeGenerateWindow
    {
        [Button("StartGenNode")]
        public void StartGenNode()
        {
            var allClass = ReflectionManager.GetTypes<GenFlowActionFromStaticFuncAttribute>();
            foreach (var classType in allClass)
            {
                if (!onlyParseTargetType.IsNullOrEmpty() && classType.Name != onlyParseTargetType)
                {
                    continue;
                }
                
                var methods = classType.GetMethods(BindingFlags.Static | BindingFlags.Public);
                string targetClassFileName = classType.Name + "Node";
                string classKeyWord = classType.Name.CutEndsWith("Static").CutEndsWith("Proxy");
                string filePath = Path.Combine(AUTO_GEN_FILE_NODE_PATH, targetClassFileName + ".cs");

                var codeBuilder = new CSharpCodeBuilder();
                if (File.Exists(filePath))
                {
                    codeBuilder = CSharpCodeBuilder.Parse(File.ReadAllText(filePath));
                }
                else
                {
                    codeBuilder.AddUsing("Design.Proxy.Utils").AddUsing("FlowCanvas").AddUsing("GameScene.FlowNode.Base")
                        .AddUsing("ParadoxNotion.Design").AddUsing("UnityEngine");
                    codeBuilder.AddNameSpace("GameScene.FlowNode.Action.AutoGen");
                }
                
                foreach (var staticMethod in methods)
                {
                    if (staticMethod.Name.EndsWith("_Get"))
                    {
                        //获取变量节点
                        var nodeName = $"{classKeyWord}_{staticMethod.Name}";
                        if (codeBuilder.TrySeekClass(nodeName))
                        {
                            continue;
                        }

                        codeBuilder.AddClass(nodeName, new[] { SyntaxKind.PublicKeyword }, new[] { "BaseFlowNode" });
                        codeBuilder.AddAttribute("Name", $"(\"{nodeName}\")");
                        codeBuilder.AddAttribute("Category", "(\"变量Var\")");

                        //Field
                        var fields = staticMethod.GetParameters();
                        for (var i = 0; i < fields.Length; i++)
                        {
                            AddValueInputOrBBParameterField(fields[i]);
                        }

                        codeBuilder.AddField($"m_value",
                            $"ValueOutput<{GetTypeName(staticMethod.ReturnType)}>",
                            new[] { SyntaxKind.PrivateKeyword });

                        //Field ReturnType

                        //Method
                        codeBuilder.AddMethod("RegisterPorts", "void",
                            new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword });
                        for (var i = 0; i < fields.Length; i++)
                        {
                            AddValueInputPort(fields[i]);
                        }

                        string[] paramNames = new string[fields.Length];
                        for (var i = 0; i < fields.Length; i++)
                        {
                            paramNames[i] = $"m_{fields[i].Name}.value";
                        }

                        var callMethodStr = CSharpCodeBuilderHelper.CombineFunctionCallInvoke(
                            $"{classType.Name}.{staticMethod.Name}", paramNames);
                        //Method Return Type
                        codeBuilder.AddMethodBody(
                            $"m_value = AddValueOutput<{GetTypeName(staticMethod.ReturnType)}>(\"Value\", () => {callMethodStr});");
                    }
                    else if (typeof(UniTask).IsAssignableFrom(staticMethod.ReturnType))
                    {
                        //异步事件节点
                        var nodeName = $"{classKeyWord}_{staticMethod.Name}";
                        if (codeBuilder.TrySeekClass(nodeName))
                        {
                            continue;
                        }
                        
                        codeBuilder.AddClass(nodeName, new[] { SyntaxKind.PublicKeyword }, new[] { "BaseFlowActionAsync" });
                        codeBuilder.AddAttribute("Name", $"(\"{nodeName}\")");
                        codeBuilder.AddAttribute("Category", "(\"行为Action\")");

                        //Field
                        var fields = staticMethod.GetParameters();
                        for (var i = 0; i < fields.Length; i++)
                        {
                            AddValueInputOrBBParameterField(fields[i]);
                        }

                        //Method
                        codeBuilder.AddMethod("RegisterPorts", "void",
                            new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword });
                        codeBuilder.AddMethodBody("base.RegisterPorts();");
                        for (var i = 0; i < fields.Length; i++)
                        {
                            AddValueInputPort(fields[i]);
                        }

                        codeBuilder.AddMethod("InvokeFunction", "UniTask",
                            new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword });
                        codeBuilder.AddMethodParameter("flow", "Flow");
                        
                        string[] paramNames = new string[fields.Length];
                        for (var i = 0; i < fields.Length; i++)
                        {
                            paramNames[i] = $"m_{fields[i].Name}.value";
                        }
                        codeBuilder.AddMethodBody("return " + CSharpCodeBuilderHelper.CombineFunctionCallInvoke(
                            $"{classType.Name}.{staticMethod.Name}", paramNames));
                    }
                    else
                    {
                        //同步事件节点
                        var nodeName = $"{classKeyWord}_{staticMethod.Name}";
                        if (codeBuilder.TrySeekClass(nodeName))
                        {
                            continue;
                        }
                        
                        codeBuilder.AddClass(nodeName, new[] { SyntaxKind.PublicKeyword }, new[] { "BaseFlowAction" });
                        codeBuilder.AddAttribute("Name", $"(\"{nodeName}\")");
                        codeBuilder.AddAttribute("Category", "(\"行为Action\")");

                        //Field
                        var fields = staticMethod.GetParameters();
                        for (var i = 0; i < fields.Length; i++)
                        {
                            AddValueInputOrBBParameterField(fields[i]);
                        }

                        //Method
                        codeBuilder.AddMethod("RegisterPorts", "void",
                            new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword });
                        codeBuilder.AddMethodBody("base.RegisterPorts();");
                        for (var i = 0; i < fields.Length; i++)
                        {
                            AddValueInputPort(fields[i]);
                        }

                        codeBuilder.AddMethod("InvokeFunction", "void",
                            new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword });
                        codeBuilder.AddMethodParameter("flow", "Flow", new[] { SyntaxKind.InKeyword });
                        string[] paramNames = new string[fields.Length];
                        for (var i = 0; i < fields.Length; i++)
                        {
                            paramNames[i] = $"m_{fields[i].Name}.value";
                        }

                        codeBuilder.AddMethodBody(CSharpCodeBuilderHelper.CombineFunctionCallInvoke(
                            $"{classType.Name}.{staticMethod.Name}", paramNames));
                    }
                }

                var text = codeBuilder.FinishBuildToFullString();
                LogUtils.Error($"生成代码:{filePath}\n{text}");
                if (!noWriteFile)
                {
                    if (File.Exists(filePath) && UnityEditor.VersionControl.Provider.enabled)
                    {
                        UnityEditor.VersionControl.Provider.Checkout(filePath, CheckoutMode.Asset);
                    }
                    File.WriteAllText(filePath, text);
                    AssetDatabase.Refresh();
                }
                
                void AddValueInputOrBBParameterField(ParameterInfo field)
                {
                    //TODO:部分参数需要用BBParameter
                    bool useValueInputOrBBParameter = true;
                    if (useValueInputOrBBParameter)
                    {
                        bool isNodeId = field.ParameterType == typeof(ulong) &&
                                        field.Name.Contains("nodeId", StringComparison.OrdinalIgnoreCase);
                        if (!isNodeId)
                        {
                            codeBuilder.AddField($"m_{field.Name}",
                                $"ValueInput<{GetTypeName(field.ParameterType)}>",
                                new[] { SyntaxKind.PrivateKeyword });
                        }
                        else
                        {
                            codeBuilder.AddField($"m_{field.Name}",
                                $"NodeIdInput",
                                new[] { SyntaxKind.PrivateKeyword });
                        }
                    }
                    else
                    {
                        //TODO:默认值没有生成
                        codeBuilder.AddField($"m_{field.Name}",
                            $"BBParameter<{GetTypeName(field.ParameterType)}>",
                            new[] { SyntaxKind.PrivateKeyword });
                    }
                }
                
                void AddValueInputPort(ParameterInfo field)
                {
                    //TODO:部分参数需要用BBParameter
                    bool useValueInputOrBBParameter = true;
                    if (useValueInputOrBBParameter)
                    {
                        if (field.Name != "nodeId")
                        {
                            codeBuilder.AddMethodBody(
                                $"m_{field.Name} = AddValueInput<{GetTypeName(field.ParameterType)}>(\"{CSharpCodeBuilderHelper.ToUpperFirstChar(field.Name)}\");");
                        }
                        else
                        {
                            codeBuilder.AddMethodBody($"m_{field.Name} = AddNodeIdInput();");
                        }
                    }
                    //BBParameter不需要生成
                }
            }
        }
    }
}