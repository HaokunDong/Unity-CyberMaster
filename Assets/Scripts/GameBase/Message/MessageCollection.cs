using System;
using System.Collections.Generic;

public interface IMessageCollection
{
    void RemoveAllMsgListener();
}

public class MessageCollection : List<(Type type, Delegate @delegate, MessageCenter messageCenter)>,IMessageCollection
{
    public MessageCollection AddListener<T>(MessageDelegate<T> action) where T : IMessage
    {
        var type = typeof(T);
        Message.AddListener(action);
        Add((type, action, null));
        return this;
    }

    public MessageCollection AddListenerLocal<T>(MessageCenter msgCenter, MessageDelegate<T> action) where T : IMessage
    {
        var type = typeof(T);
        msgCenter.AddListener(action);
        Add((type, action, msgCenter));
        return this;
    }
    
    public MessageCollection RemoveMsgListener<T>() where T : IMessage
    {
        var type = typeof(T);
        var index = FindIndex(pair => pair.type == type);
        if (index >= 0)
        {
            var pair = this[index];
            if (pair.messageCenter != null)
            {
                pair.messageCenter?.RemoveListener(pair.type, pair.@delegate);
            }
            else
            {
                Message.RemoveListener(pair.type, pair.@delegate);
            }
            RemoveAt(index);
        }
        return this;
    }
    
    public void RemoveAllMsgListener()
    {
        foreach (var pair in this)
        {
            if (pair.messageCenter != null)
            {
                pair.messageCenter?.RemoveListener(pair.type, pair.@delegate);
            }
            else
            {
                Message.RemoveListener(pair.type, pair.@delegate);
            }
        }
        Clear();
    }
}