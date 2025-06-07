using System.Diagnostics;
using System.Reflection;
using System.Text;
using SqlSugar;
using SqlSugar.TDengine;

namespace sqlsugar_test;

public static class SqlSugarExtensions
{
    public static STagInsertChildTable<T> ToSTableChild<T>(this IInsertable<T> thisValue, Func<string, T, string> getChildTableNamefunc) where T : class, new()
    {
        STagInsertChildTable<T> result = new STagInsertChildTable<T>();
        result.thisValue = thisValue;
        result.Context = ((InsertableProvider<T>)thisValue).Context;
        result.getChildTableNamefunc = getChildTableNamefunc;
        return result;
    }
}

public class STagInsertChildTable<T> where T : class, new()
{
    internal IInsertable<T> thisValue;
    internal Func<string, T, string> getChildTableNamefunc;

    internal SqlSugarProvider Context;


    public List<string> GetExecuteSql(int pageSize = 500)
    {
        var reuslt = new List<string>();
        var provider = (InsertableProvider<T>)thisValue;
        var inserObjects = provider.InsertObjs;
        var attr = GetCommonSTableAttribute(typeof(T).GetCustomAttribute<STableAttribute>());
        Check.ExceptionEasy(attr == null || attr?.Tag1 == null, $"", $"{nameof(T)}缺少特性STableAttribute和Tag1");
        // 根据所有非空的 Tag 进行分组
        var ignoreColumns = GetTagNames(inserObjects.First(), attr).ToArray();
        StringBuilder sb;
        var sTableName = provider.SqlBuilder.GetTranslationColumnName(attr.STableName);
        var entityInfo = this.Context.EntityMaintenance.GetEntityInfo<T>();
        var columnInfos = entityInfo.Columns.Where(t => !ignoreColumns.Contains(t.DbColumnName)).ToList();
        var columnNames = string.Join(",", columnInfos.Select(t => $"'{t.DbColumnName.ToLower()}'").ToList());

        this.Context.Utilities.PageEach(inserObjects, pageSize, pageItems =>
        {
            var sw = Stopwatch.StartNew();
            var groups = GetGroupInfos(pageItems.ToArray(), attr);
            sb = new StringBuilder();
            sb.Append("INSERT INTO");
            foreach (var group in groups)
            {
                var groupList = group.ToList();
                var childTableName = getChildTableNamefunc(attr.STableName, groupList.First());
                sb.AppendLine();
                List<string> tagValues = GetTagValues(groupList, attr);
                string.Join("_", tagValues.Select(v => v.ToSqlFilter()));
                var tagString = string.Join(",", tagValues.Select(v => $"'{v.ToSqlFilter()}'"));
                sb.Append($"`{childTableName}` "); //指定子表名称
                sb.Append($" USING {sTableName} tags({tagString}) "); //tags值
                sb.AppendLine($"({columnNames})"); //列名
                sb.Append("VALUES (");
                foreach (var item in groupList)
                {
                    var columnValues = GetColumnValues(item, columnInfos);
                    sb.Append(columnValues);
                }

                // await this.Context.Ado.ExecuteCommandAsync($"CREATE TABLE IF NOT EXISTS {childTableName} USING {sTableName} TAGS ({tagString})");
                // await this.Context.Insertable(pageItems).IgnoreColumns(ignoreColumns).AS(childTableName).ExecuteCommandAsync();
                sb.Append(")");
            }

            reuslt.Add(sb.ToString());
            sw.Stop();
            Console.WriteLine($"前奏用时 {sw.Elapsed.TotalSeconds} s");
            sw.Restart();
            // await provider.Ado.ExecuteCommandAsync(sb.ToString());
            sw.Stop();
            Console.WriteLine($"插入{pageItems.Count}用时 {sw.Elapsed.TotalMilliseconds} ms");
        });

        return reuslt;
    }

    public async Task<int> ExecuteInsertAsync(int pageSize = 500)
    {
        var provider = (InsertableProvider<T>)thisValue;
        var inserObjects = provider.InsertObjs;
        var attr = GetCommonSTableAttribute(typeof(T).GetCustomAttribute<STableAttribute>());
        Check.ExceptionEasy(attr == null || attr?.Tag1 == null, $"", $"{nameof(T)}缺少特性STableAttribute和Tag1");
        // 根据所有非空的 Tag 进行分组
        var ignoreColumns = GetTagNames(inserObjects.First(), attr).ToArray();
        StringBuilder sb;
        var sTableName = provider.SqlBuilder.GetTranslationColumnName(attr.STableName);
        var entityInfo = this.Context.EntityMaintenance.GetEntityInfo<T>();
        var columnInfos = entityInfo.Columns.Where(t => !ignoreColumns.Contains(t.DbColumnName)).ToList();
        var columnNames = string.Join(",", columnInfos.Select(t => $"'{t.DbColumnName.ToLower()}'").ToList());

        this.Context.Utilities.PageEachAsync(inserObjects, pageSize, async pageItems =>
        {
            var sw = Stopwatch.StartNew();
            var groups = GetGroupInfos(pageItems.ToArray(), attr);
            sb = new StringBuilder();
            sb.Append("INSERT INTO");
            foreach (var group in groups)
            {
                var groupList = group.ToList();
                var childTableName = getChildTableNamefunc(attr.STableName, groupList.First());
                sb.AppendLine();
                List<string> tagValues = GetTagValues(groupList, attr);
                string.Join("_", tagValues.Select(v => v.ToSqlFilter()));
                var tagString = string.Join(",", tagValues.Select(v => $"'{v.ToSqlFilter()}'"));
                sb.Append($"`{childTableName}` "); //指定子表名称
                sb.Append($" USING {sTableName} tags({tagString}) "); //tags值
                sb.AppendLine($"({columnNames})"); //列名
                sb.Append("VALUES (");
                foreach (var item in groupList)
                {
                    var columnValues = GetColumnValues(item, columnInfos);
                    sb.Append(columnValues);
                }

                sb.Append(")");
            }

            sw.Stop();
            Console.WriteLine($"扩展中分页生成sql用时 {sw.Elapsed.TotalSeconds} s");
            sw.Restart();
            try
            {
                await provider.Ado.ExecuteCommandAsync(sb.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            sw.Stop();
            Console.WriteLine($"扩展中分页插入{pageItems.Count}用时 {sw.Elapsed.TotalMilliseconds} ms");
        });

        return inserObjects.Count();
    }

    private string GetColumnValues(T obj, List<EntityColumnInfo> columnInfos)
    {
        var list = new List<string>();
        var objType = typeof(T);
        foreach (var entityColumnInfo in columnInfos)
        {
            var type = entityColumnInfo.UnderType;
            var propertyName = entityColumnInfo.PropertyName;
            var objValue = objType.GetProperty(propertyName)!.GetValue(obj);
            if (objValue == null)
            {
                list.Add("NULL");
                continue;
            }

            // if(entityColumnInfo.UnderType)
            if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                if (entityColumnInfo.InsertServerTime)
                {
                    list.Add("NOW()");
                }
                else
                {
                    list.Add($"'{objValue.ObjToDate():yyyy-MM-dd HH:mm:ss.ffffff}'");
                }
            }
            else if (type == typeof(bool))
            {
                list.Add($"{(objValue.ObjToBool() ? "1" : "0")}");
            }
            else if (type == typeof(string))
            {
                list.Add($"'{objValue.ObjToString()}'");
            }
            else
            {
                list.Add($"{objValue.ObjToString()}");
            }
            // else if(entityColumnInfo)
            // {
            //     list.Add("");
            // }
        }

        return string.Join(",", list);
    }


    private static List<string> GetTagValues(List<T> pageItems, STableAttribute attr)
    {
        var tagValues = new List<string>();
        var obj = pageItems.First();
        if (attr.Tag1 != null)
            tagValues.Add(obj.GetType().GetProperty(attr.Tag1)?.GetValue(obj)?.ToString());

        if (attr.Tag2 != null)
            tagValues.Add(obj.GetType().GetProperty(attr.Tag2)?.GetValue(obj)?.ToString());

        if (attr.Tag3 != null)
            tagValues.Add(obj.GetType().GetProperty(attr.Tag3)?.GetValue(obj)?.ToString());

        if (attr.Tag4 != null)
            tagValues.Add(obj.GetType().GetProperty(attr.Tag4)?.GetValue(obj)?.ToString());
        return tagValues.Where(v => !string.IsNullOrEmpty(v)).ToList();
    }

    private static List<string> GetTagNames(T obj, STableAttribute attr)
    {
        var tagValues = new List<string>();
        if (attr.Tag1 != null)
            tagValues.Add(attr.Tag1);

        if (attr.Tag2 != null)
            tagValues.Add(attr.Tag2);

        if (attr.Tag3 != null)
            tagValues.Add(attr.Tag3);

        if (attr.Tag4 != null)
            tagValues.Add(attr.Tag4);
        return tagValues;
    }

    private static IEnumerable<IGrouping<string, T>> GetGroupInfos(T[] inserObjects, STableAttribute? attr)
    {
        var groups = inserObjects.GroupBy(it =>
        {
            // 动态生成分组键
            var groupKey = new List<string>();

            if (attr.Tag1 != null)
                groupKey.Add(it.GetType().GetProperty(attr.Tag1)?.GetValue(it)?.ToString());

            if (attr.Tag2 != null)
                groupKey.Add(it.GetType().GetProperty(attr.Tag2)?.GetValue(it)?.ToString());

            if (attr.Tag3 != null)
                groupKey.Add(it.GetType().GetProperty(attr.Tag3)?.GetValue(it)?.ToString());

            if (attr.Tag4 != null)
                groupKey.Add(it.GetType().GetProperty(attr.Tag4)?.GetValue(it)?.ToString());

            // 将非空的 Tag 值用下划线连接作为分组键
            return string.Join("_", groupKey.Where(k => !string.IsNullOrEmpty(k)));
        });
        return groups;
    }

    private STableAttribute GetCommonSTableAttribute(STableAttribute sTableAttribute)
    {
        return SqlSugar.TDengine.UtilMethods.GetCommonSTableAttribute(this.Context, sTableAttribute);
    }
}