using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using CodeBuild;
using Cysharp.Threading.Tasks;
using Design.Proxy.Utils;
using Everlasting.Extend;
using GameBase.Log;
using GameBase.Reflection;
using GameBase.Utils;
using Microsoft.CodeAnalysis.CSharp;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace GamePlayTool.Editor.FlowCanvasTool
{
    //将static函数代码翻译为FlowNode代码
    //该工具只能用于辅助编写Flow节点代码，生成后还是得自己看情况再做些调整
    public partial class NodeCodeGenerateWindow : OdinEditorWindow
    {
        [MenuItem("GamePlay/代码生成/FlowCanvas节点生成窗口")]
        private static void ShowCustomDefaultSceneWindow()
        {
            var window = GetWindow<NodeCodeGenerateWindow>();
            window.Show();
        }

        private const string AUTO_GEN_FILE_NODE_PATH = "Assets/Scripts/GamePlayRoot/GameScene/FlowNode/Action/AutoGen";
        private const string AUTO_GEN_FILE_STATIC_UTILS_PATH = "Assets/Scripts/Design/Proxy/Utils";

        private static string GetTypeName(Type type)
        {
            return CSharpCodeBuilderHelper.GetTypeNameKeyword(type);
        }

        [Tooltip("不写文件，只打印LOG")]
        public bool noWriteFile = false;

        [Tooltip("仅导出指定名称的类，调试用")]
        public string onlyParseTargetType;
        
        [Button("StartGenStaticFunc")]
        public void StartGenStaticFunc()
        {
            //将GamePlay的interface转换为static函数
            var allClass = ReflectionManager.GetTypes<GenFuncStaticFromIProxyAttribute>();
            foreach (var interfaceType in allClass)
            {
                if (!onlyParseTargetType.IsNullOrEmpty() && interfaceType.Name != onlyParseTargetType)
                {
                    continue;
                }
                
                string className = interfaceType.Name.CutStartsWith("I") + "Static";
                string targetFilePath = Path.Combine(AUTO_GEN_FILE_STATIC_UTILS_PATH,
                    className + ".cs");
                CSharpCodeBuilder codeBuilder = File.Exists(targetFilePath)
                    ? CSharpCodeBuilder.Parse(File.ReadAllText(targetFilePath))
                    : new CSharpCodeBuilder();

                //生成对应类
                if (!codeBuilder.TrySeekFirstClass())
                {
                    codeBuilder.AddUsing("UnityEngine");
                    codeBuilder.AddUsing("GameScene.GamePlay");
                    codeBuilder.AddNameSpace("Design.Proxy.Utils");
                    codeBuilder.AddClass(className,
                        new SyntaxKind[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword });
                    codeBuilder.AddAttribute("GenFlowActionFromStaticFunc",$"(typeof({interfaceType.Name}))");
                }

                //生成GetSource
                if (!codeBuilder.HasMethod("GetSource"))
                {
                    //内容得自己去实现
                    codeBuilder.AddMethod("GetSource", interfaceType.Name, new SyntaxKind[]
                    {
                        SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword
                    });
                    codeBuilder.AddMethodParameter("nodeId", "ulong");
                    codeBuilder.AddMethodBody("return null;");
                }

                var properties = interfaceType.GetProperties();
                foreach (var property in properties)
                {
                    var getMethod = property.GetGetMethod();
                    if (getMethod != null)
                    {
                        string methodName = property.Name + "_Get";
                        if (!codeBuilder.HasMethod(methodName))
                        {
                            codeBuilder.AddMethod(methodName, GetTypeName(getMethod.ReturnType), new[]
                            {
                                SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword
                            });
                            codeBuilder.AddMethodParameter("nodeId", "ulong");
                            codeBuilder.AddMethodBody("var source = GetSource(nodeId);");
                            codeBuilder.AddMethodBody($"return source.{property.Name};");
                        }
                    }

                    var setMethod = property.GetSetMethod();
                    if (setMethod != null)
                    {
                        string methodName = property.Name + "_Set";
                        if (!codeBuilder.HasMethod(methodName))
                        {
                            codeBuilder.AddMethod(methodName, "void", new[]
                            {
                                SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword
                            });
                            codeBuilder.AddMethodParameter("nodeId", "ulong");
                            codeBuilder.AddMethodParameter("value", GetTypeName(property.PropertyType));
                            codeBuilder.AddMethodBody("var source = GetSource(nodeId);");
                            codeBuilder.AddMethodBody($"source.{property.Name} = value;");
                        }
                    }
                }

                var methods = interfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (var method in methods)
                {
                    //过滤get、set自动生成的方法
                    if (method.IsSpecialName) continue;
                    string methodName = method.Name;
                    if (!codeBuilder.HasMethod(methodName))
                    {
                        codeBuilder.AddMethod(methodName, GetTypeName(method.ReturnType), new[]
                        {
                            SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword
                        });
                        codeBuilder.AddMethodParameter("nodeId", "ulong");
                        var methodParams = method.GetParameters();
                        foreach (var methodParam in methodParams)
                        {
                            codeBuilder.AddMethodParameter(methodParam.Name, GetTypeName(methodParam.ParameterType), null, 
                                methodParam.HasDefaultValue ? methodParam.DefaultValue : null);
                        }
                        
                        codeBuilder.AddMethodBody("var source = GetSource(nodeId);");
                        string[] paramNames = new string[methodParams.Length];
                        for (var i = 0; i < methodParams.Length; i++)
                        {
                            paramNames[i] = methodParams[i].Name;
                        }

                        {
                            string body = CSharpCodeBuilderHelper.CombineFunctionCallInvoke(
                                $"source.{methodName}", paramNames);
                            if (method.ReturnType != typeof(void))
                            {
                                body = "return " + body;
                            }
                            codeBuilder.AddMethodBody(body);
                        }
                    }
                }

                string text = codeBuilder.FinishBuildToFullString();
                LogUtils.Debug($"生成代码:{targetFilePath}\n{text}");
                if (!noWriteFile)
                {
                    if (File.Exists(targetFilePath) && UnityEditor.VersionControl.Provider.enabled)
                    {
                        UnityEditor.VersionControl.Provider.Checkout(targetFilePath, CheckoutMode.Both);
                    }
                    File.WriteAllText(targetFilePath, text);
                }
                AssetDatabase.Refresh();
            }
        }
    }
}