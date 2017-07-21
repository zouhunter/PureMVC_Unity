﻿using System;
using System.Collections.Generic;

namespace UnityEngine
{

    public class View : IView
    {
        protected IList<IMediator> m_mediatorMap;
        protected IDictionary<string, List<IObserverBase>> m_observerMap;
        protected static volatile IView m_instance;

        protected View()
        {
            m_mediatorMap = new List<IMediator>();
            m_observerMap = new Dictionary<string, List<IObserverBase>>();
            InitializeView();
        }
        public static IView Instance
        {
            get
            {
                if (m_instance == null)
                {
                    if (m_instance == null) m_instance = new View();
                }

                return m_instance;
            }
        }
        protected virtual void InitializeView()
        {
            
        }
        /// <summary>
        /// 注册成为观察者
        /// </summary>
        /// <param name="obName"></param>
        /// <param name="observer"></param>
        public void RegisterObserver(string eventName, IObserverBase observer)
        {
            if (m_observerMap.ContainsKey(eventName))
            {
                if (!m_observerMap[eventName].Contains(observer))
                {
                    m_observerMap[eventName].Add(observer);
                }
            }
            else
            {
                m_observerMap.Add(eventName, new List<IObserverBase>() { observer });
            }
        }
        /// <summary>
        /// 通知所有观察者
        /// </summary>
        /// <param name="notify"></param>
        public void NotifyObservers<T>(INotification<T> noti)
        {
            IList<IObserverBase> observers = null;

            if (m_observerMap.ContainsKey(noti.ObserverName))
            {
                IList<IObserverBase> observers_ref = m_observerMap[noti.ObserverName];
                observers = new List<IObserverBase>(observers_ref);
            }

            if (observers != null)
            {
                for (int i = 0; i < observers.Count; i++)
                {
                    IObserverBase observer = observers[i];
                    if (observer is IObserver<T>)
                    {
                        (observer as IObserver<T>).NotifyObserver(noti);
                    }
                    else if (observer is IObserver)
                    {
                        (observer as IObserver).NotifyObserver(noti);
                    }
                }
            }
        }
        /// <summary>
        /// 通知所有观察者
        /// </summary>
        /// <param name="notify"></param>
        public void NotifyObservers(INotification noti)
        {
            IList<IObserverBase> observers = null;

            if (m_observerMap.ContainsKey(noti.ObserverName))
            {
                IList<IObserverBase> observers_ref = m_observerMap[noti.ObserverName];
                observers = new List<IObserverBase>(observers_ref);
            }

            if (observers != null)
            {
                for (int i = 0; i < observers.Count; i++)
                {
                    IObserverBase observer = observers[i];
                    if (observer is IObserver)
                    {
                        (observer as IObserver).NotifyObserver(noti);
                    }
                }
            }
        }
        /// <summary>
        /// 将指定的观察者移除
        /// </summary>
        /// <param name="name"></param>
        public void RemoveObserver(string eventName, object notifyContext)
        {
            // the observer list for the notification under inspection
            if (m_observerMap.ContainsKey(eventName))
            {
                IList<IObserverBase> observers = m_observerMap[eventName];

                for (int i = 0; i < observers.Count; i++)
                {
                    if (observers[i].CompareNotifyContext(notifyContext))
                    {
                        observers.RemoveAt(i);
                        break;
                    }
                }

                if (observers.Count == 0)
                {
                    m_observerMap.Remove(eventName);
                }
            }
        }
        /// <summary>
        /// 将所有的观察者移除
        /// </summary>
        /// <param name="name"></param>
        public void RemoveObservers(string eventName)
        {
            if (m_observerMap.ContainsKey(eventName))
            {
                m_observerMap.Remove(eventName);
            }
        }
        /// <summary>
        /// 注册mediator
        /// </summary>
        /// <param name="notify"></param>
        public void RegisterMediator<T>(IMediator<T> mediator)
        {
            if (m_mediatorMap.Contains(mediator)) return;

            // Register the Mediator for retrieval by name
            m_mediatorMap.Add(mediator);

            // Get Notification interests, if any.
            IList<string> interests = mediator.ListNotificationInterests();
            // Register Mediator as an observer for each of its notification interests
            if (interests.Count > 0)
            {
                // Create Observer
                IObserver<T> observer = new Observer<T>((x) => (mediator as IMediator<T>).HandleNotification(x.Body), mediator);

                // Register Mediator as Observer for its list of Notification interests
                for (int i = 0; i < interests.Count; i++)
                {
                    RegisterObserver(interests[i], observer);
                }
            }
        }

        /// <summary>
        /// 是否含有观察者
        /// </summary>
        /// <param name="observerName"></param>
        /// <returns></returns>
        public bool HasObserver(string observerName)
        {
            return m_observerMap.ContainsKey(observerName);
        }

        public void RemoveMediator(IMediator mediator)
        {
            if (!m_mediatorMap.Contains(mediator)) return;

            IList<string> interests = mediator.ListNotificationInterests();

            for (int i = 0; i < interests.Count; i++)
            {
                RemoveObserver(interests[i], mediator);
            }

            m_mediatorMap.Remove(mediator);
        }
    }
}