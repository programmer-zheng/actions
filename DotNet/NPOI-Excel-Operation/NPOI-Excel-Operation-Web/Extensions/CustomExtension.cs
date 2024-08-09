using System.Reflection;
using NPOI_Excel_Operation_Web.Dto;

namespace NPOI_Excel_Operation_Web.Extensions;

public class CustomExtension
{

	public static List<T> DynamicGroup<T>(List<T> list, List<DynamicGroupPropertyOperationDto> propertyOperationList, params string[] groupByProperties) where T : new()
	{
		var result = new List<T>();
		//if (list?.Count == 0 || excelconfig?.ColumnModel?.Count == 0)
		//{
		//	return result;
		//}
		var grouped = list!.GroupBy(item => string.Join("|", groupByProperties.Select(groupByProperty => item.GetType().GetProperty(groupByProperty).GetValue(item, null)))).ToList();
		var properties = typeof(T).GetProperties();
		//var exportColumns = excelconfig.ColumnModel!.Where(item => !string.IsNullOrWhiteSpace(item.Column))
		//		.Select(item => item.Column!)
		//		.Distinct()
		//		.ToHashSet<string>();
		string propertyName = string.Empty;
		foreach (var item in grouped)
		{
			var output = new T();
			foreach (var property in properties)
			{
				propertyName = property.Name;
				//if (!exportColumns.Contains(propertyName))
				//{
				//	continue;
				//}
				string distinctByProperty = null;
				DynamicGroupLinqOperatorEnum operate = DynamicGroupLinqOperatorEnum.Sum;
				var propertyOperation = propertyOperationList.FirstOrDefault(t => t.PropertyName == propertyName);
				if (propertyOperation != null)
				{
					operate = propertyOperation.Operate;
					distinctByProperty = propertyOperation.DistinctByPropertyName;
				}
				object val = null;
				if (property.PropertyType == typeof(decimal?) || property.PropertyType == typeof(decimal))
				{
					val = GetDecimalValue(item, property, operate, distinctByProperty);
				}
				else if (property.PropertyType == typeof(int?) || property.PropertyType == typeof(int))
				{
					val = GetIntValue(item, property, operate, distinctByProperty);
				}
				else if (property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(DateTime))
				{
					val = GetDateTimeValue(item, property, operate);
				}
				else if (property.PropertyType == typeof(string))
				{
					val = GetStringValues(item, property, operate);
				}
				else if (property.PropertyType == typeof(Dictionary<string, string>))
				{
					val = GetDictionaryValue(item, property);
				}
				property.SetValue(output, val);
			}
			result.Add(output);
		}
		return result;
	}

	private static Dictionary<string, string> GetDictionaryValue<T>(IEnumerable<T> group, PropertyInfo property)
	{
		var dic = new Dictionary<string, string>();
		foreach (var item in group)
		{
			var dictionary = (Dictionary<string, string>)property.GetValue(item);
			if (dictionary != null)
			{
				foreach (var kvp in dictionary)
				{
					if (!dic.ContainsKey(kvp.Key))
					{
						dic[kvp.Key] = kvp.Value;
					}
				}
			}
		}
		return dic;
	}

	private static decimal? GetDecimalValue<T>(IEnumerable<T> group, PropertyInfo property, DynamicGroupLinqOperatorEnum operation, string distinctByProperty = null)
	{
		switch (operation)
		{
			case DynamicGroupLinqOperatorEnum.Max:
				return group.Max(x => (decimal?)property.GetValue(x));
			case DynamicGroupLinqOperatorEnum.Min:
				return group.Min(x => (decimal?)property.GetValue(x));
			case DynamicGroupLinqOperatorEnum.Sum:
				return group.Sum(x => (decimal?)property.GetValue(x));
			case DynamicGroupLinqOperatorEnum.First:
				return (decimal?)property.GetValue(group.First());
			case DynamicGroupLinqOperatorEnum.DistinctSum:
				return group
			   .GroupBy(x => typeof(T).GetProperty(distinctByProperty).GetValue(x))
			   .Select(g => g.First())
			   .Sum(x => (decimal?)property.GetValue(x));
			default:
				return group.Sum(x => (decimal?)property.GetValue(x));
		}
	}

	private static int? GetIntValue<T>(IEnumerable<T> group, PropertyInfo property, DynamicGroupLinqOperatorEnum operation, string distinctByProperty = null)
	{
		switch (operation)
		{
			case DynamicGroupLinqOperatorEnum.Max:
				return group.Max(x => (int?)property.GetValue(x));
			case DynamicGroupLinqOperatorEnum.Min:
				return group.Min(x => (int?)property.GetValue(x));
			case DynamicGroupLinqOperatorEnum.Sum:
				return group.Sum(x => (int?)property.GetValue(x));
			case DynamicGroupLinqOperatorEnum.First:
				return (int?)property.GetValue(group.First());
			case DynamicGroupLinqOperatorEnum.DistinctSum:
				return group
			   .GroupBy(x => typeof(T).GetProperty(distinctByProperty).GetValue(x))
			   .Select(g => g.First())
			   .Sum(x => (int?)property.GetValue(x));
			default:
				return group.Sum(x => (int?)property.GetValue(x));
		}
	}

	private static DateTime? GetDateTimeValue<T>(IEnumerable<T> group, PropertyInfo property, DynamicGroupLinqOperatorEnum operation)
	{
		switch (operation)
		{
			case DynamicGroupLinqOperatorEnum.Max:
				return group.Max(x => (DateTime?)property.GetValue(x));
			case DynamicGroupLinqOperatorEnum.Min:
				return group.Min(x => (DateTime?)property.GetValue(x));
			case DynamicGroupLinqOperatorEnum.First:
				return (DateTime?)property.GetValue(group.First());
			default:
				return group.Max(x => (DateTime?)property.GetValue(x));
		}
	}

	private static string GetStringValues<T>(IEnumerable<T> group, PropertyInfo property, DynamicGroupLinqOperatorEnum operation)
	{
		switch (operation)
		{
			case DynamicGroupLinqOperatorEnum.First:
				return (string)property.GetValue(group.First());
			case DynamicGroupLinqOperatorEnum.Max:
				return (string)property.GetValue(group.Max());
			case DynamicGroupLinqOperatorEnum.Concat:
			default:
				// TODO 合并后，为空的是否需要删除，好处是，不会多出一个逗号开头的，坏处是会误以为相同
				return string.Join(",", group.Select(x => property.GetValue(x)?.ToString()).Distinct().Where(t => !string.IsNullOrWhiteSpace(t)));
		}
	}
}