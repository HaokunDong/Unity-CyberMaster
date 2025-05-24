using BehaviourTrees;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class SMAndBTGraphView : GraphView
{
    private IVisualElementScheduledItem stateCheckSchedule;
    private IVisualElementScheduledItem glowSchedule;

    private EnemyState previousEnemyState;
    private Dictionary<string, EnemyState> latestEDict = null;
    private EnemyStateMachine lastESM = null;

    private PlayerState previousPlayerState;
    private Dictionary<string, PlayerState> latestPDict = null;
    private PlayerStateMachine lastPSM = null;

    public SMAndBTGraphView()
    {
        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
    }

    public void EnableAutoRedrawOnResize()
    {
        RegisterCallback<GeometryChangedEvent>(evt =>
        {
            if (latestEDict != null && lastESM != null)
                DrawNodesOnEdges(latestEDict, lastESM);
            else if(latestPDict != null && lastPSM != null)
                DrawNodesOnEdges(latestPDict, lastPSM);
        });
    }

    public void StopAutoStateCheck()
    {
        stateCheckSchedule?.Pause(); // 立即停止执行
        stateCheckSchedule = null;
    }

    public void StartAutoStateCheck(float intervalSeconds = 0.2f)
    {
        StopAutoStateCheck();

        stateCheckSchedule = schedule.Execute(() =>
        {
            if (lastESM != null && latestEDict != null)
            {
                if (lastESM.currentState != previousEnemyState)
                {
                    previousEnemyState = lastESM.currentState;
                    DrawNodesOnEdges(latestEDict, lastESM);
                }
            }

            if (lastPSM != null && latestPDict != null)
            {
                if (lastPSM.currentState != previousPlayerState)
                {
                    previousPlayerState = lastPSM.currentState;
                    DrawNodesOnEdges(latestPDict, lastPSM);
                }
            }

        }).Every((long)(intervalSeconds * 1000));
    }

    public void DrawNodesOnEdges(Dictionary<string, EnemyState> dict, EnemyStateMachine sm)
    {
        previousPlayerState = null;
        latestPDict = null;
        lastPSM = null;
        previousEnemyState = null;
        latestEDict = dict;
        lastESM = sm;

        // 清理旧节点
        var toRemove = new List<GraphElement>();
        foreach (var element in graphElements)
            if (element is Node)
                toRemove.Add(element);
        foreach (var node in toRemove)
            RemoveElement(node);

        int count = dict.Count;
        if (count == 0 || layout.width <= 0 || layout.height <= 0)
            return;

        var nodeDataList = dict.ToList();
        float nodeWidth = Mathf.Clamp(layout.width * 0.1f, 80f, 150f);
        float nodeHeight = Mathf.Clamp(layout.height * 0.08f, 40f, 100f);

        // 动态计算内收距离
        float edgePadding = Mathf.Clamp(layout.width * 0.1f, 50f, 200f);
        float cornerPadding = Mathf.Clamp(layout.width * 0.1f, 50f, 200f);

        // 平均分配节点数
        int perSide = count / 4;
        int remainder = count % 4;

        int[] sideCounts = new int[4];
        for (int i = 0; i < 4; i++)
            sideCounts[i] = perSide + (i < remainder ? 1 : 0);

        float width = layout.width;
        float height = layout.height;

        int index = 0;

        // 计算四边长度（排除内收和角落留白）
        float topLength = width - 2 * edgePadding - 2 * cornerPadding;
        float rightLength = height - 2 * edgePadding - 2 * cornerPadding;
        float bottomLength = width - 2 * edgePadding - 2 * cornerPadding;
        float leftLength = height - 2 * edgePadding - 2 * cornerPadding;

        // Top 边
        for (int i = 0; i < sideCounts[0]; i++, index++)
        {
            float x = edgePadding + cornerPadding;
            if (sideCounts[0] > 1)
                x += i * topLength / (sideCounts[0] - 1);
            else
                x = width / 2f;

            float y = edgePadding;
            var kv = nodeDataList[index];
            AddElement(CreateNode(kv.Value, sm, kv.Key, new Vector2(x, y), nodeWidth, nodeHeight));
        }

        // Right 边
        for (int i = 0; i < sideCounts[1]; i++, index++)
        {
            float x = width - nodeWidth - edgePadding;
            float y = edgePadding + cornerPadding;
            if (sideCounts[1] > 1)
                y += i * rightLength / (sideCounts[1] - 1);
            else
                y = height / 2f;

            var kv = nodeDataList[index];
            AddElement(CreateNode(kv.Value, sm, kv.Key, new Vector2(x, y), nodeWidth, nodeHeight));
        }

        // Bottom 边
        for (int i = 0; i < sideCounts[2]; i++, index++)
        {
            float x = edgePadding + cornerPadding;
            if (sideCounts[2] > 1)
                x += i * bottomLength / (sideCounts[2] - 1);
            else
                x = width / 2f;

            float y = height - nodeHeight - edgePadding;
            var kv = nodeDataList[index];
            AddElement(CreateNode(kv.Value, sm, kv.Key, new Vector2(x, y), nodeWidth, nodeHeight));
        }

        // Left 边
        for (int i = 0; i < sideCounts[3]; i++, index++)
        {
            float x = edgePadding;
            float y = edgePadding + cornerPadding;
            if (sideCounts[3] > 1)
                y += i * leftLength / (sideCounts[3] - 1);
            else
                y = height / 2f;

            var kv = nodeDataList[index];
            AddElement(CreateNode(kv.Value, sm, kv.Key, new Vector2(x, y), nodeWidth, nodeHeight));
        }

        FrameAll();
    }

    public void DrawNodesOnEdges(Dictionary<string, PlayerState> dict, PlayerStateMachine sm)
    {
        previousPlayerState = null;
        latestPDict = dict;
        lastPSM = sm;
        previousEnemyState = null;
        latestEDict = null;
        lastESM = null;

        // 清理旧节点
        var toRemove = new List<GraphElement>();
        foreach (var element in graphElements)
            if (element is Node)
                toRemove.Add(element);
        foreach (var node in toRemove)
            RemoveElement(node);

        int count = dict.Count;
        if (count == 0 || layout.width <= 0 || layout.height <= 0)
            return;

        var nodeDataList = dict.ToList();
        float nodeWidth = Mathf.Clamp(layout.width * 0.1f, 80f, 150f);
        float nodeHeight = Mathf.Clamp(layout.height * 0.08f, 40f, 100f);

        // 动态计算内收距离
        float edgePadding = Mathf.Clamp(layout.width * 0.1f, 50f, 200f);
        float cornerPadding = Mathf.Clamp(layout.width * 0.1f, 50f, 200f);

        // 平均分配节点数
        int perSide = count / 4;
        int remainder = count % 4;

        int[] sideCounts = new int[4];
        for (int i = 0; i < 4; i++)
            sideCounts[i] = perSide + (i < remainder ? 1 : 0);

        float width = layout.width;
        float height = layout.height;

        int index = 0;

        // 计算四边长度（排除内收和角落留白）
        float topLength = width - 2 * edgePadding - 2 * cornerPadding;
        float rightLength = height - 2 * edgePadding - 2 * cornerPadding;
        float bottomLength = width - 2 * edgePadding - 2 * cornerPadding;
        float leftLength = height - 2 * edgePadding - 2 * cornerPadding;

        // Top 边
        for (int i = 0; i < sideCounts[0]; i++, index++)
        {
            float x = edgePadding + cornerPadding;
            if (sideCounts[0] > 1)
                x += i * topLength / (sideCounts[0] - 1);
            else
                x = width / 2f;

            float y = edgePadding;
            var kv = nodeDataList[index];
            AddElement(CreateNode(kv.Value, sm, kv.Key, new Vector2(x, y), nodeWidth, nodeHeight));
        }

        // Right 边
        for (int i = 0; i < sideCounts[1]; i++, index++)
        {
            float x = width - nodeWidth - edgePadding;
            float y = edgePadding + cornerPadding;
            if (sideCounts[1] > 1)
                y += i * rightLength / (sideCounts[1] - 1);
            else
                y = height / 2f;

            var kv = nodeDataList[index];
            AddElement(CreateNode(kv.Value, sm, kv.Key, new Vector2(x, y), nodeWidth, nodeHeight));
        }

        // Bottom 边
        for (int i = 0; i < sideCounts[2]; i++, index++)
        {
            float x = edgePadding + cornerPadding;
            if (sideCounts[2] > 1)
                x += i * bottomLength / (sideCounts[2] - 1);
            else
                x = width / 2f;

            float y = height - nodeHeight - edgePadding;
            var kv = nodeDataList[index];
            AddElement(CreateNode(kv.Value, sm, kv.Key, new Vector2(x, y), nodeWidth, nodeHeight));
        }

        // Left 边
        for (int i = 0; i < sideCounts[3]; i++, index++)
        {
            float x = edgePadding;
            float y = edgePadding + cornerPadding;
            if (sideCounts[3] > 1)
                y += i * leftLength / (sideCounts[3] - 1);
            else
                y = height / 2f;

            var kv = nodeDataList[index];
            AddElement(CreateNode(kv.Value, sm, kv.Key, new Vector2(x, y), nodeWidth, nodeHeight));
        }

        FrameAll();
    }

    private Node CreateNode(PlayerState state, PlayerStateMachine sm, string title, Vector2 position, float width, float height)
    {
        var node = new Node
        {
            title = title,
        };

        node.titleContainer.style.borderBottomWidth = 1;
        node.titleContainer.style.borderTopWidth = 1;
        node.titleContainer.style.borderLeftWidth = 1;
        node.titleContainer.style.borderRightWidth = 1;
        node.titleContainer.style.borderBottomColor = Color.white;

        node.SetPosition(new Rect(position, new Vector2(width, height)));

        if (state == sm.currentState)
        {
            // 背景
            node.titleContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 1f);

            // 设置初始边框颜色
            Color baseColor = new Color(0.2f, 0.2f, 1f, 1f);
            node.titleContainer.style.borderLeftColor = baseColor;
            node.titleContainer.style.borderRightColor = baseColor;
            node.titleContainer.style.borderTopColor = baseColor;
            node.titleContainer.style.borderBottomColor = baseColor;

            // 设置动画：让透明度循环变化产生“发光”效果
            AnimateBorderGlow(node.titleContainer, baseColor);
        }
        else
        {
            node.titleContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        return node;
    }


    private Node CreateNode(EnemyState state, EnemyStateMachine sm, string title, Vector2 position, float width, float height)
    {
        var node = new Node
        {
            title = title,
        };

        node.titleContainer.style.borderBottomWidth = 1;
        node.titleContainer.style.borderTopWidth = 1;
        node.titleContainer.style.borderLeftWidth = 1;
        node.titleContainer.style.borderRightWidth = 1;
        node.titleContainer.style.borderBottomColor = Color.white;

        node.SetPosition(new Rect(position, new Vector2(width, height)));

        if (state == sm.currentState)
        {
            // 背景
            node.titleContainer.style.backgroundColor = new Color(0.1f, 0.3f, 0.1f, 1f);

            // 设置初始边框颜色
            Color baseColor = new Color(0.2f, 1f, 0.2f, 1f);  // 荧光绿
            node.titleContainer.style.borderLeftColor = baseColor;
            node.titleContainer.style.borderRightColor = baseColor;
            node.titleContainer.style.borderTopColor = baseColor;
            node.titleContainer.style.borderBottomColor = baseColor;

            // 设置动画：让透明度循环变化产生“发光”效果
            AnimateBorderGlow(node.titleContainer, baseColor);
        }
        else
        {
            node.titleContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        var field = state.GetType().GetField("behaviourTree", BindingFlags.NonPublic | BindingFlags.Instance);
        var bt = field?.GetValue(state) as BTNode;
        if (bt != null)
        {
            var icon = new Label("BT");
            icon.style.unityTextAlign = TextAnchor.MiddleCenter;
            icon.style.fontSize = 14;
            icon.style.marginLeft = 4;
            icon.style.marginTop = 2;
            icon.style.width = 20;
            icon.style.height = 20;
            icon.style.backgroundColor = new Color(0, 0, 0, 0); // transparent
            icon.style.color = Color.yellow;

            node.titleContainer.Add(icon);

            //if(state == sm.currentState)
            //{
            //    DrawBTTree(bt);
            //}
        }

        return node;
    }

    private void AnimateBorderGlow(VisualElement target, Color baseColor)
    {
        float cycleDuration = 2f; // 完整一次淡入淡出周期（秒）
        float minAlpha = 0.4f;
        float maxAlpha = 1.0f;
        float startTime = Time.realtimeSinceStartup;

        glowSchedule?.Pause();
        glowSchedule = target.schedule.Execute(() =>
        {
            float t = (Time.realtimeSinceStartup - startTime) / cycleDuration;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, 0.5f * (1 + Mathf.Sin(t * Mathf.PI * 2)));

            Color glowColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            target.style.borderLeftColor = glowColor;
            target.style.borderRightColor = glowColor;
            target.style.borderTopColor = glowColor;
            target.style.borderBottomColor = glowColor;

        }).Every(16);
    }

    private void DrawBTTree(BTNode root)
    {
        if (root == null) return;

        float nodeWidth = 160f;
        float nodeHeight = 60f;
        float horizontalSpacing = 40f;
        float verticalSpacing = 80f;

        Vector2 startPosition = new Vector2(layout.width / 2f - nodeWidth / 2f, layout.height / 2f - 200);

        Dictionary<BTNode, Node> createdNodes = new();
        Dictionary<BTNode, Vector2> positions = new();

        // 递归创建节点并记录位置
        void Traverse(BTNode node, int depth, int indexAtLevel)
        {
            Vector2 position = new Vector2(
                startPosition.x + indexAtLevel * (nodeWidth + horizontalSpacing),
                startPosition.y + depth * (nodeHeight + verticalSpacing)
            );
            positions[node] = position;

            var viewNode = CreateBTNode(node.GetType().Name, position, nodeWidth, nodeHeight);
            createdNodes[node] = viewNode;
            AddElement(viewNode);

            var children = GetBTChildren(node);
            for (int i = 0; i < children.Count; i++)
            {
                Traverse(children[i], depth + 1, i);
            }
        }

        Traverse(root, 0, 0);

        // 创建连接线
        foreach (var kvp in createdNodes)
        {
            var node = kvp.Key;
            var parentNode = kvp.Value;
            foreach (var child in GetBTChildren(node))
            {
                if (createdNodes.TryGetValue(child, out var childNode))
                {
                    var edge = parentNode.outputContainer.Q<Port>().ConnectTo(childNode.inputContainer.Q<Port>());
                    AddElement(edge);
                }
            }
        }
    }

    private Node CreateBTNode(string title, Vector2 position, float width, float height)
    {
        var node = new Node
        {
            title = title
        };
        node.titleContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.3f, 1f);
        node.SetPosition(new Rect(position, new Vector2(width, height)));

        var input = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        input.portName = "";
        node.inputContainer.Add(input);

        var output = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        output.portName = "";
        node.outputContainer.Add(output);

        node.RefreshExpandedState();
        node.RefreshPorts();

        return node;
    }

    // 提取 BTNode 的子节点（假设使用 List<BTNode> children）
    private List<BTNode> GetBTChildren(BTNode node)
    {
        if (node == null) return new List<BTNode>();

        var type = node.GetType();

        var field = type.GetField("children", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var value = field.GetValue(node);
            if (value is BTNode[] array)
            {
                return array.ToList();
            }
        }

        // 如果还有其它组合器类型可以继续加判断
        return new List<BTNode>();
    }
}
