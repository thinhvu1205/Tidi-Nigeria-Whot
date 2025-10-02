
public class ICallFunc
{
    public delegate void Func1(); // () => {...}
    public delegate void Func2<in T1>(T1 t1); // (T1) => {...}
    public delegate void Func3<in T1, in T2>(T1 t1, T2 t2); // (T1, T2) => {...}
    public delegate void Func4<in T1, in T2, in T3>(T1 t1, T2 t2, T3 t3); // (T1, T2, T3) => {...}
    public delegate void Func5<in T1, in T2, in T3, in T4>(T1 t1, T2 t2, T3 t3, T4 t4); // (T1, T2, T3, T4) => {...}
    public delegate T1 Func6<out T1>(); // () => {...return T1;}
    public delegate T2 Func7<in T1, out T2>(T1 t2); // (T1) => {...return T2;}
    public delegate T3 Func8<in T1, in T2, out T3>(T1 t1, T2 t2); // (T1, T2) => {...return T3;}

    public delegate void Func2Obj(object t1);
    public delegate void Func3Obj(object t1, object t2);
    public delegate void Func4Obj(object t1, object t2, object t3);
    public delegate void Func5Obj(object t1, object t2, object t3, object t4);
    public delegate object Func6Obj();
    public delegate object Func7Obj(object t2);
    public delegate object Func8Obj(object t2, object t3);
}
