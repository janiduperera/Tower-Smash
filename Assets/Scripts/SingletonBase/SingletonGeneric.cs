﻿using System;

public class SingletonGeneric<T> where T : class, new()
{
    private static readonly Lazy<T> instance = new Lazy<T>(() => new T());

    public static T Instance
    {
        get
        {
            return instance.Value;
        }
    }

    private SingletonGeneric() { }
}
