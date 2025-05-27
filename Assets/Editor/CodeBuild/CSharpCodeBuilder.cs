using System;
using System.IO;
using System.Linq;
using Everlasting.Extend;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeBuild
{
    public class CSharpCodeBuilder
    {
        //构建语法树
        private CompilationUnitSyntax m_syntaxFactory;
        private NamespaceDeclarationSyntax m_namespace;
        private ClassDeclarationSyntax m_class;
        private MethodDeclarationSyntax m_method;

        public CSharpCodeBuilder()
        {
            m_syntaxFactory = SyntaxFactory.CompilationUnit();
        }

        public static CSharpCodeBuilder Parse(string sourceText, bool seekFirstNameSpace = true)
        {
            var codeBuilder = new CSharpCodeBuilder();
            codeBuilder.m_syntaxFactory = SyntaxFactory.ParseSyntaxTree(sourceText).GetCompilationUnitRoot();
            //定位到第一个nameSpace，一般一个文件只有一个nameSpace
            if (seekFirstNameSpace)
            {
                codeBuilder.m_namespace = codeBuilder.m_syntaxFactory.Members.First(syntax => syntax is NamespaceDeclarationSyntax) as NamespaceDeclarationSyntax;
            
                if (codeBuilder.m_namespace != null)
                {
                    codeBuilder.m_syntaxFactory = codeBuilder.m_syntaxFactory.WithMembers(codeBuilder.m_syntaxFactory.Members.Remove(codeBuilder.m_namespace));
                }
            }
            return codeBuilder;
        }

        public CSharpCodeBuilder AddUsing(string usingName)
        {
            if (m_syntaxFactory.Usings.All(u => u.Name.ToString() != usingName))
            {
                m_syntaxFactory = m_syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(usingName)));
            }
            return this;
        }
        
        public CSharpCodeBuilder AddUsing(Type type)
        {
            AddUsing(type.Namespace);
            return this;
        }

        //添加注释 TODO：还不完善
        public CSharpCodeBuilder AddTrivia(string description)
        {
            m_syntaxFactory = m_syntaxFactory.WithLeadingTrivia(SyntaxFactory.ParseTrailingTrivia(description));
            return this;
        }

        #region namespace
        public CSharpCodeBuilder AddNameSpace(string nameSpace)
        {
            EndNameSpace();
            m_namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(nameSpace)).NormalizeWhitespace();
            return this;
        }

        private CSharpCodeBuilder EndNameSpace()
        {
            if (m_namespace != null)
            {
                m_syntaxFactory = m_syntaxFactory.AddMembers(m_namespace);
                m_namespace = null;
            }
            return this;
        }

        public CSharpCodeBuilder SeekNameSpace(string name)
        {
            EndNameSpace();
            try
            {
                m_namespace = m_syntaxFactory.Members.First(syntax => syntax is NamespaceDeclarationSyntax namespaceSyntax && namespaceSyntax.Name.ToString() == name) as NamespaceDeclarationSyntax;
            
                if (m_namespace != null)
                {
                    m_syntaxFactory = m_syntaxFactory.WithMembers(m_syntaxFactory.Members.Remove(m_namespace));
                }
            }
            catch (InvalidOperationException){}
            
            return this;
        }
        #endregion

        #region class

        public CSharpCodeBuilder AddClass(string className, SyntaxKind[] syntaxKinds = null, string[] baseTypes = null)
        {
            EndMethod();
            EndClass();
            m_class = SyntaxFactory.ClassDeclaration(className);
            if (syntaxKinds != null)
            {
                m_class = m_class.AddModifiers(syntaxKinds.Select(kind => SyntaxFactory.Token(kind)).ToArray());
            }

            if (baseTypes != null)
            {
                m_class = m_class.AddBaseListTypes(baseTypes.Select<string, BaseTypeSyntax>(
                    type => SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(type))).ToArray());
            }
            
            return this;
        }

        public CSharpCodeBuilder EndClass()
        {
            if (m_class != null)
            {
                m_namespace = m_namespace?.AddMembers(m_class);
                m_class = null;
            }
            
            return this;
        }

        public bool TrySeekFirstClass()
        {
            EndClass();
            if (m_namespace != null)
            {
                try
                {
                    m_class = m_namespace.Members.First(syntax => syntax is ClassDeclarationSyntax) as ClassDeclarationSyntax;
                    if (m_class != null)
                    {
                        m_namespace = m_namespace.WithMembers(m_namespace.Members.Remove(m_class));
                        return true;
                    }
                }
                catch (InvalidOperationException){}
            }
            else
            {
                try
                {
                    m_class = m_syntaxFactory.Members.First(syntax => syntax is ClassDeclarationSyntax) as ClassDeclarationSyntax;
                    if (m_class != null)
                    {
                        m_syntaxFactory = m_syntaxFactory.WithMembers(m_syntaxFactory.Members.Remove(m_class));
                        return true;
                    }
                }
                catch (InvalidOperationException){}
            }

            return false;
        }
        
        public bool TrySeekClass(string className)
        {
            if (m_class != null)
            {
                if (m_class.Identifier.Text == className) return true;
            }

            if (m_namespace != null)
            {
                try
                {
                    if (m_namespace.Members.First(syntax =>
                        (syntax is ClassDeclarationSyntax classSyntax) && classSyntax.Identifier.Text == className) is ClassDeclarationSyntax findClass)
                    {
                        m_namespace = m_namespace.WithMembers(m_namespace.Members.Remove(findClass));
                        EndClass();
                        m_class = findClass;
                        return true;
                    }
                }
                catch (InvalidOperationException){}
            }

            return false;
        }
        
        public CSharpCodeBuilder SeekClass(string className)
        {
            TrySeekClass(className);
            return this;
        }

        #endregion
        
        #region attribute

        public CSharpCodeBuilder AddAttribute(string attributeName, string argumentList = null)
        {
            if (m_class != null)
            {
                var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
                attributeList = _AddToAttributeList(attributeList, attributeName, argumentList);

                m_class = m_class.AddAttributeLists(SyntaxFactory.AttributeList(attributeList));
            }

            return null;
        }

        public CSharpCodeBuilder AddAttributeToField(string fieldName, string attributeName, string argumentList = null)
        {
            if (m_class != null)
            {
                var field = m_class.Members.First(syntax => syntax is FieldDeclarationSyntax f && f.Declaration.Variables.Any(vds => vds.Identifier.Text == fieldName)) as FieldDeclarationSyntax;
                var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
                attributeList = _AddToAttributeList(attributeList, attributeName, argumentList);
                
                m_class = m_class.ReplaceNode(field, field.AddAttributeLists(SyntaxFactory.AttributeList(attributeList)));
            }
            return this;
        }

        private SeparatedSyntaxList<AttributeSyntax> _AddToAttributeList(SeparatedSyntaxList<AttributeSyntax> attributeList, string attributeName,
            string argumentList)
        {
            if (argumentList != null)
            {
                return attributeList.Add(SyntaxFactory.Attribute(SyntaxFactory.ParseName(attributeName),
                    SyntaxFactory.ParseAttributeArgumentList(argumentList)));
            }
            else
            {
                return attributeList.Add(SyntaxFactory.Attribute(SyntaxFactory.ParseName(attributeName)));
            }
        }

        #endregion
        
        #region field

        public CSharpCodeBuilder AddField(string name, string typeName, SyntaxKind[] keyword = null)
        {
            var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(typeName))
                .AddVariables(SyntaxFactory.VariableDeclarator(name));
            var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration);
            if (keyword != null)
            {
                fieldDeclaration = fieldDeclaration.AddModifiers(keyword.Select(SyntaxFactory.Token).ToArray());
            }
            
            m_class = m_class.AddMembers(fieldDeclaration);
            return this;
        }
        
        #endregion
        
        #region method

        public CSharpCodeBuilder AddMethod(string methodName, string returnName, SyntaxKind[] keywords = null)
        {
            EndMethod();
            m_method = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnName), methodName);
            if (keywords != null)
            {
                m_method = m_method.AddModifiers(keywords.Select(kind => SyntaxFactory.Token(kind)).ToArray());
            }
            return this;
        }

        public CSharpCodeBuilder AddMethodParameter(string paramName, string typeName, SyntaxKind[] keywords = null, object defaultValue = null)
        {
            if (m_method != null)
            {
                var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(paramName))
                    .WithType(SyntaxFactory.ParseTypeName(typeName));
                if (keywords != null)
                {
                    parameter = parameter.AddModifiers(keywords.Select(kind => SyntaxFactory.Token(kind)).ToArray());
                }

                //TODO:还未支持
                // if (defaultValue != null)
                // {
                //     var defaultClauseSyntax = SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, 
                //         SyntaxFactory.Literal(defaultValue.ToString())));
                //     parameter = parameter.WithDefault(defaultClauseSyntax);
                // }
                
                m_method = m_method.AddParameterListParameters(parameter);
            }
            
            return this;
        }

        public CSharpCodeBuilder AddMethodBody(string body, string comment = null)
        {
            if (m_method != null)
            {
                var statementSyntax = SyntaxFactory.ParseStatement(body);
                if (!comment.IsNullOrEmpty())
                {
                    statementSyntax = statementSyntax.WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(comment));
                }
                m_method = m_method.AddBodyStatements(statementSyntax);
            }
            return this;
        }

        public CSharpCodeBuilder EndMethod()
        {
            if (m_method != null)
            {
                AddMethodBody("\n");
                m_class = m_class?.AddMembers(m_method);
                m_method = null;
            }
            return this;
        }

        public bool HasMethod(string methodName)
        {
            if (m_class != null)
            {
                return m_class.Members.Any(syntax =>
                    syntax is MethodDeclarationSyntax methodDeclarationSyntax &&
                    methodDeclarationSyntax.Identifier.Text == methodName);
            }

            return false;
        }
        
        public CSharpCodeBuilder RemoveMethod(string methodName)
        {
            if (m_class != null)
            {
                try
                {
                    if (m_class.Members.First(syntax =>
                        (syntax is MethodDeclarationSyntax methodDeclarationSyntax) && methodDeclarationSyntax.Identifier.Text == methodName) is MethodDeclarationSyntax findMethod)
                    {
                        m_class = m_class.WithMembers(m_namespace.Members.Remove(findMethod));
                    }
                }
                catch (InvalidOperationException){}
            }

            return this;
        }
        
        #endregion
        
        public string FinishBuildToFullString()
        {
            EndMethod();
            EndClass();
            EndNameSpace();
            return m_syntaxFactory.NormalizeWhitespace().ToFullString();
        }

        // public static string Test()
        // {
        //     var codeBuilder = new CSharpCodeBuilder();
        //     // codeBuilder.Parse(File.ReadAllText(
        //     //     @"Assets\Scripts\GamePlayRoot\GameScene\FlowNode\Action\AutoGen\SceneItemProxyStaticNodes.cs"));
        //     codeBuilder.AddUsing("Design.Proxy.Utils").AddUsing("FlowCanvas").AddUsing("GameScene.FlowNode.Base")
        //         .AddUsing("ParadoxNotion.Design");
        //     codeBuilder.AddNameSpace("GameScene.FlowNode.Action.AutoGen");
        //     codeBuilder.AddClass("SceneItem_CanInteractState_Set", new [] { SyntaxKind.PublicKeyword }, new []{"BaseFlowAction"});
        //
        //     codeBuilder.AddAttributes("Name", "(\"SceneItem_CanInteractState_Set\")");
        //     codeBuilder.AddAttributes("Category", "(\"行为\")");
        //     
        //     codeBuilder.AddField("m_nodeId", "ValueInput<ulong>", new [] { SyntaxKind.PrivateKeyword });
        //     codeBuilder.AddField("m_setValue", "ValueInput<bool>", new [] { SyntaxKind.PrivateKeyword });
        //     
        //     codeBuilder.AddMethod("RegisterPorts", "void", new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword });
        //     codeBuilder.AddMethodBody("base.RegisterPorts();").AddMethodBody("m_nodeId = AddValueInput<ulong>(\"NodeId\");")
        //         .AddMethodBody("m_setValue = AddValueInput<bool>(\"Set\");");
        //     
        //     codeBuilder.AddMethod("InvokeFunction", "void", new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.OverrideKeyword });
        //     codeBuilder.AddMethodParameter("flow", "Flow", new[] { SyntaxKind.InKeyword });
        //     codeBuilder.AddMethodBody("SceneItemProxyStatic.CanInteractState_Set(m_nodeId.value, m_setValue.value);");
        //
        //     codeBuilder.AddTrivia("//该代码为自动生成");
        //     return codeBuilder.GenFullString();
        // }
    }
}