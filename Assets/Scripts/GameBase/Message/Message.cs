using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Everlasting.Base;
using GameBase.Log;
using GameBase.Utils;
using UnityEngine;

public interface IMessage
{
}

public interface IMessageIgnoreDebugLog
{
}

public interface IAsyncMessage
{
}

//in可以避免struct每次函数调用拷贝
public delegate void MessageDelegate<T>(in T msg) where T : IMessage;

public delegate void MessageDelegate();

public delegate UniTask MessageDelegateAsync<T>(in T message) where T : IAsyncMessage;

public class MessageCenter : IDisposable
{
    private Dictionary<Type, ReentrantLinkedList<Delegate>> typeHandlers =
        new Dictionary<Type, ReentrantLinkedList<Delegate>>();

    public void AddListener<T>(MessageDelegate<T> callback) where T : IMessage
    {
        AddListener(typeof(T), callback);
    }

    private void AddListener(Type type, Delegate callback)
    {
        if (callback == null)
        {
            UnityEngine.Debug.LogError("Failed to add Message Listener because the given callback is null!");
            return;
        }

        if (typeHandlers.TryGetValue(type, out ReentrantLinkedList<Delegate> messageHandlers))
        {
            messageHandlers.Add(callback);
        }
        else
        {
            typeHandlers.Add(type, new ReentrantLinkedList<Delegate> { callback });
        }
    }

    public void RemoveListener<T>(MessageDelegate<T> callback) where T : IMessage
    {
        RemoveListener(typeof(T), callback);
    }

    public void RemoveListener(Type type, Delegate callback)
    {
        if (typeHandlers.TryGetValue(type, out ReentrantLinkedList<Delegate> messageHandlers))
        {
            var messageNode = messageHandlers.Find(cb => cb == callback);
            if (messageNode != null)
            {
                messageHandlers.Remove(messageNode);
            }
        }
    }

#if DEBUG_ASSIST_ENABLE
    private static StringBuilder s_debugStringBuilder = new StringBuilder(100);
#endif
    public void SendMessage<T>(in T e) where T : IMessage
    {
#if DEBUG_ASSIST_ENABLE
        if (LogUtils.IsLogChannelEnable(LogChannel.Message) && !(e is IMessageIgnoreDebugLog))
        {
            s_debugStringBuilder.Append("<color=yellow>");
            s_debugStringBuilder.Append(e);
            s_debugStringBuilder.Append("</color>");
            s_debugStringBuilder.Append(' ');
            ReflectionUtils.GetFieldsLog(e, s_debugStringBuilder);
            LogUtils.Debug(s_debugStringBuilder.ToString(), LogChannel.Message);
            s_debugStringBuilder.Clear();
        }
#endif
        if (typeHandlers.TryGetValue(typeof(T), out ReentrantLinkedList<Delegate> messageHandlers))
        {
            InvokeMessageHandles<T>(messageHandlers, in e);
        }
    }

    //同时发局部和全局事件
    public void SendLocalAndGlobal<T>(in T e) where T : IMessage
    {
        SendMessage(in e);
        Message.Send(in e);
    }

    private void InvokeMessageHandles<T>(ReentrantLinkedList<Delegate> messageHandlers, in T e)
        where T : IMessage
    {
        foreach (Delegate messageHandler in messageHandlers)
        {
            try
            {
                if (messageHandler is MessageDelegate<T> @delegate)
                {
                    @delegate.Invoke(in e);
                }
                else
                {
                    ((MessageDelegate)messageHandler).Invoke();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    private readonly Dictionary<Type, ReentrantLinkedList<Delegate>> m_messagesAsync = new Dictionary<Type, ReentrantLinkedList<Delegate>>();

    //异步消息
    public async UniTask SendAsync<T>(T message) where T : IAsyncMessage
    {
        if (m_messagesAsync.TryGetValue(typeof(T), out var actions))
        {
            var tempList = ListPool<UniTask>.Fetch();
            foreach (var cb in actions)
            {
                if (cb is MessageDelegateAsync<T> action)
                {
                    UniTask task;
                    try
                    {
                        task = action.Invoke(in message);
                        tempList.Add(task);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        task = UniTask.CompletedTask;
                    }
                }
            }

            try
            {
                await UniTask.WhenAll(tempList);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            ListPool<UniTask>.Release(tempList);
        }
    }

    public void AddListenerAsync<T>(MessageDelegateAsync<T> action) where T : IAsyncMessage
    {
        var type = typeof(T);
        if (m_messagesAsync.TryGetValue(type, out ReentrantLinkedList<Delegate> actions))
        {
            actions.Add(action);
        }
        else
        {
            m_messagesAsync.Add(type, new ReentrantLinkedList<Delegate> { action });
        }
    }

    public void RemoveListenerAsync<T>(MessageDelegateAsync<T> action) where T : IAsyncMessage
    {
        var type = typeof(T);
        if (m_messagesAsync.TryGetValue(type, out ReentrantLinkedList<Delegate> actions))
        {
            actions.Remove(action);
            if (actions.Count == 0)
            {
                m_messagesAsync.Remove(type);
            }
        }
    }

    public void Dispose()
    {
        typeHandlers.Clear();
    }
}

/// <summary>
/// A global messaging system that can be used to communicate between Controllers or other classes.
/// </summary>
public static class Message
{
    private static MessageCenter s_golbalMessageCenter = new MessageCenter();

    /// <summary>
    /// A delegation type used to callback when messages are sent and received.
    /// </summary>
    /// <param name="callerType">The caller type of the message.</param>
    /// <param name="handlerType">The handler type of the message.</param> 
    /// <param name="messageType">The type of the sent message.</param>
    /// <param name="messageName">The name of the sent message.</param>
    /// <param name="handlerMethodName">The name of the method handling the sent message.</param>
    // public delegate void OnMessageHandleDelegate(Type callerType, Type handlerType, Type messageType, string messageName, string handlerMethodName);

    /// <summary>
    /// Called when a message is sent and handled. Only works when in the Unity editor.
    /// </summary>
    // public static OnMessageHandleDelegate OnMessageHandle;

    // private static Dictionary<string, List<Delegate>> strHandlers = new Dictionary<string, List<Delegate>>();
    private static Dictionary<Type, ReentrantLinkedList<Delegate>> typeHandlers =
        new Dictionary<Type, ReentrantLinkedList<Delegate>>();

    /// <summary>
    /// Adds a listener that triggers the given callback when a message of the given type and name is received.
    /// </summary>
    /// <typeparam name="T">The message type that will be listened to.</typeparam>
    /// <param name="messageName">The message name that will be listened to.</param>
    /// <param name="callback">The callback that is triggered when the message is received.</param>
    public static void AddListener<T>(MessageDelegate<T> callback) where T : IMessage
    {
        s_golbalMessageCenter.AddListener<T>(callback);
    }

    /// <summary>
    /// Removes a listener that would trigger the given callback when a message of the given type is received.
    /// </summary>
    /// <typeparam name="T">The message type that is being listened to.</typeparam>
    /// <param name="callback">The callback that is triggered when the message is received.</param>
    public static void RemoveListener<T>(MessageDelegate<T> callback) where T : IMessage
    {
        s_golbalMessageCenter.RemoveListener<T>(callback);
    }

    public static void RemoveListener(Type type, Delegate callback)
    {
        s_golbalMessageCenter.RemoveListener(type, callback);
    }

    /// <summary>
    /// Sends a message of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    /// <param name="message">The instance of the message.</param>
    public static void Send<T>(in T message) where T : IMessage
    {
        s_golbalMessageCenter.SendMessage<T>(in message);
    }

    public static void AddListenerAsync<T>(MessageDelegateAsync<T> callback) where T : IAsyncMessage
    {
        s_golbalMessageCenter.AddListenerAsync(callback);
    }
    
    public static void RemoveListenerAsync<T>(MessageDelegateAsync<T> callback) where T : IAsyncMessage
    {
        s_golbalMessageCenter.RemoveListenerAsync(callback);
    }
    
    public static UniTask SendAsync<T>(in T message) where T : IAsyncMessage
    {
        return s_golbalMessageCenter.SendAsync(message);
    }
}