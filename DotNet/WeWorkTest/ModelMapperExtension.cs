using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WeWorkTest
{
    public static class ModelMapperExtension
    {
        /// <summary>
        /// Model映射
        /// 注意映射规则的Key为目标对象属性, Value为源对象属性
        /// </summary>
        public static TTargetModel ToMapperModel<TOriginalModel, TTargetModel>(this TOriginalModel originalModel, Dictionary<string, string> mapperRule)
            where TTargetModel : class, new()
            where TOriginalModel : class

        {
            if (mapperRule == null || mapperRule.Count == 0)
                throw new Exception($"Mapper rule was null or empty.");

            if (mapperRule.Any(t => string.IsNullOrWhiteSpace(t.Value)))
                throw new Exception($"Mapper rule was not correct. Rule:[{mapperRule.ToJson()}].");

            if (originalModel == null)
                throw new Exception($"Model was null while mapper.");

            var targetModel = new TTargetModel();
            var originalProperties = originalModel.GetType().GetProperties();
            var targetProperties = targetModel.GetType().GetProperties();

            foreach (var mapperRuleItem in mapperRule)
            {
                var originalPropertyInfo = originalProperties.FirstOrDefault(t => t.Name == mapperRuleItem.Value);
                var targetPropertyInfo = targetProperties.FirstOrDefault(t => t.Name == mapperRuleItem.Key);
                if (originalPropertyInfo == null || targetPropertyInfo == null) continue;
                var originalValue = originalPropertyInfo.GetValue(originalModel, null);
                var targetValue = targetPropertyInfo.GetValue(targetModel, null);
                bool isEqual = targetPropertyInfo.PropertyType.Name == originalPropertyInfo.PropertyType.Name;
                if (isEqual)
                    isEqual = originalValue.IsEqual(targetValue, originalPropertyInfo.PropertyType.Name);

                if (!isEqual)
                {
                    try
                    {
                        if (targetPropertyInfo.PropertyType.Name != originalPropertyInfo.PropertyType.Name)
                        {
                            originalValue = originalValue.GetTargetTypeValue(targetPropertyInfo.PropertyType);
                        }

                        targetPropertyInfo.SetValue(targetModel, originalValue);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine( $"Field Mapper Error,Field:{mapperRuleItem.Value},origin value:{originalValue}");
                    }
                }
            }

            return targetModel;
        }

        /// <summary>
        /// 转化对应的属性值
        /// </summary>
        public static object GetTargetTypeValue(this object value, Type propertyType)
        {
            if (value == null) return null;
            switch (propertyType.Name)
            {
                case nameof(DateTime):
                    return Convert.ToDateTime(value);
                case nameof(Int64):
                    return Convert.ToInt64(value);
                case nameof(Int32):
                    return Convert.ToInt32(value);
                case nameof(Boolean):
                    return Convert.ToBoolean(value);
                case nameof(Double):
                    return Convert.ToDouble(value);
                case nameof(Single):
                    return Convert.ToSingle(value);
                default:
                    if (propertyType.IsEnum)
                    {
                        return Enum.Parse(propertyType, value.ToString());
                    }
                    return value.ToString();
            }
        }

        /// <summary>
        /// 判定属性值是否相等
        /// </summary>
        public static bool IsEqual(this object originalValue, object targetValue, string propertyTypeName)
        {
            if (originalValue == null && targetValue == null) return true;
            if (originalValue == null || targetValue == null) return false;

            switch (propertyTypeName)
            {
                case nameof(DateTime):
                    return Convert.ToDateTime(originalValue) == Convert.ToDateTime(targetValue);
                case nameof(Int64):
                    return Convert.ToInt64(originalValue) == Convert.ToInt64(targetValue);
                case nameof(Int32):
                    return Convert.ToInt32(originalValue) == Convert.ToInt32(targetValue);
                case nameof(Boolean):
                    return Convert.ToBoolean(originalValue) == Convert.ToBoolean(targetValue);
                case nameof(Double):
                    return Convert.ToDouble(originalValue) == Convert.ToDouble(targetValue);
                case nameof(Single):
                    return Convert.ToSingle(originalValue) == Convert.ToSingle(targetValue);
                default:
                    return originalValue.ToString() == targetValue.ToString();
            }
        }
    }
}
