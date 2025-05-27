using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CodeBuild;
using Microsoft.CodeAnalysis.CSharp;
using Sirenix.OdinInspector;

namespace GamePlayTool.Editor.FlowCanvasTool
{
    public partial class NodeCodeGenerateWindow
    {
        private static List<Type> needParseMessage = new List<Type>() { };
        private const string AUTO_GEN_MESSAGE_NODE_PATH = "Assets/Scripts/GamePlayRoot/GameScene/FlowNode/Event/MessageEventNode.cs";
        
        //从messageType自动生成flowcanvas监听节点
        [Button("GenMessage")]
        private void GenMessage()
        {
            var codeBuilder = new CSharpCodeBuilder();
            if (File.Exists(AUTO_GEN_MESSAGE_NODE_PATH))
            {
                codeBuilder = CSharpCodeBuilder.Parse(File.ReadAllText(AUTO_GEN_MESSAGE_NODE_PATH));
            }
            else
            {
                codeBuilder.AddUsing("Design.Proxy.Utils").AddUsing("FlowCanvas").AddUsing("GameScene.FlowNode.Base")
                    .AddUsing("ParadoxNotion.Design").AddUsing("UnityEngine");
                codeBuilder.AddNameSpace("GameScene.FlowNode.Event");
            }
            
            foreach (var msgType in needParseMessage)
            {
                var className = "Message_" + msgType.Name;
                if (codeBuilder.TrySeekClass(className))
                {
                    continue;
                }
                
                codeBuilder.AddClass(className, new[] { SyntaxKind.PublicKeyword }, new[] { $"BaseFlowEventMessage<{msgType.Name}>" });
                codeBuilder.AddAttribute("Name", $"(\"{className}\")");
                codeBuilder.AddAttribute("Category", "(\"事件Event\")");
                
                //获取ValueOutPut信息
                var fields = msgType.GetFields(BindingFlags.Instance | BindingFlags.Public);
                
                //TODO:没写完
            }
        }
    }
}