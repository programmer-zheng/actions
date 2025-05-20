using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core;
public static class EnumHelper
{
    public static string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var desAttribute = field.GetCustomAttribute<DescriptionAttribute>();
        if (desAttribute != null)
            return desAttribute.Description;
        return value.ToString();
    }

    // 根据枚举的字符串值和指定的枚举类型获取描述
    public static string GetEnumDescription(string enumValue, Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("指定的类型必须是枚举类型", nameof(enumType));

        // 将字符串转换为枚举值
        var enumField = enumType.GetField(enumValue);
        if (enumField == null)
            return enumValue;  // 如果没有匹配的值，返回原始值

        var desAttribute = enumField.GetCustomAttribute<DescriptionAttribute>();
        if (desAttribute != null)
            return desAttribute.Description;
        return enumValue;
    }

    public static string GetEnumDescription<T>(int value) where T : Enum
    {
        // 获取枚举类型的所有字段（包括字段名称）
        var enumType = typeof(T);
        foreach (var field in enumType.GetFields())
        {
            if (field.IsLiteral) // 判断是否是常量
            {
                // 比较值
                if ((int)field.GetValue(null) == value)
                {
                    return field.Name; // 返回枚举项的名称
                }
            }
        }
        return null; // 如果没有找到对应的枚举项
    }

    public static string GetEnumDescription<T>(string value) where T : Enum
    {
        try
        {
            // 获取枚举类型的所有字段（包括字段名称）
            var enumType = typeof(T);
            foreach (var field in enumType.GetFields())
            {
                if (field.IsLiteral) // 判断是否是常量
                {
                    // 比较值
                    if ((int)field.GetValue(null) == Convert.ToInt32(value))
                    {
                        return field.Name; // 返回枚举项的名称
                    }
                }
            }
            return value; // 如果没有找到对应的枚举项
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.ToString());
            return value;
        }
    }


    /// <summary>
    /// 将字符串分割成枚举值列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static IList<T> SplitStringAndParseToEnums<T>(this string source, char separator = ',')
        where T : struct, Enum
    {
        return source
            .Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Enum.TryParse<T>(s, true, out var t) ? (T?)t : null)
            .Where(t => t.HasValue)
            .Select(t => t.Value)
            .Distinct()
            .ToList();
    }
}
