using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//对字典类的TayGetValue( )方法的扩展
/// <summary>
/// 尝试根据key得到value，得到返回value，否则返回null
/// this Dictionary<Tkey,Tvalue> dict   这个字典表示我们要获取值value的字典
/// 扩展方法规定类必须是一个静态类.里面包含的所有方法都必须是静态方法。扩展方法被定义为静态方法，但它们是通过实例方法语法进行调用的。 它们的第一个参数指定该方法作用于哪个类型，并且该参数以 this 修饰符为前缀。
/// </summary>
public static class DictionaryExtension
{
    public static Tvalue TayGet<Tkey, Tvalue>(this Dictionary<Tkey, Tvalue> dict, Tkey key)
    {
        Tvalue value;
        dict.TryGetValue(key, out value);
        return value;
    }
}