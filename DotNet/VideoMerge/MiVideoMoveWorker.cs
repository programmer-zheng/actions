using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl.Triggers;
using Volo.Abp.BackgroundWorkers.Quartz;

namespace VideoMerge
{
    /// <summary>
    /// 小米室外摄像机视频合并
    /// </summary>
    public class MiVideoMoveWorker : QuartzBackgroundWorkerBase
    {
        public class MergeDto
        {
            public string VideoType { get; set; }

            public string FileName { get; set; }

            public string FileFullPath { get; set; }

            public DateOnly Date { get; set; }
        }


        /// <summary>
        /// 搜索模式（指定后缀或*）
        /// </summary>
        private readonly string _searchPattern;

        private readonly VideoMergeConfigOption _configOption;

        public MiVideoMoveWorker(IOptions<VideoMergeConfigOption> configureOptions)
        {
            JobDetail = JobBuilder.Create<MiVideoMoveWorker>().WithIdentity(nameof(MiVideoMoveWorker)).Build();
            Trigger = TriggerBuilder.Create().WithIdentity(nameof(MiVideoMoveWorker))
                // .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(10, 0))
                .WithSimpleSchedule(s => s.WithIntervalInHours(12))
                .StartNow()
                .Build();
            _configOption = configureOptions.Value;
            _searchPattern = $"*{_configOption.VideoSuffix}";
        }


        public override async Task Execute(IJobExecutionContext context)
        {
            if (!Directory.Exists(_configOption.BaseDirectory))
            {
                Logger.LogError($"视频存储目录：{_configOption.BaseDirectory} 不存在，无法继续操作");
                return;
            }


            // 获取根目录下的文件
            var rootFiles = GetRootFiles(_configOption.BaseDirectory);

            var dates = rootFiles.Select(t => t.Date).Distinct().ToList();
            CreateFolder(_configOption.BaseDirectory, dates);

            // 按日期分组
            var groupList = rootFiles
                .Where(t => t.Date < DateOnly.FromDateTime(DateTime.Today))
                .GroupBy(t => new { t.Date })
                .Select(t => new
                {
                    t.Key.Date,
                    files = t.ToList()
                })
                .ToList();

            foreach (var item in groupList)
            {
                // 将相应日期的视频文件移到对应日期的文件夹下
                foreach (var videoFile in item.files)
                {
                    var destFile = Path.Combine(_configOption.BaseDirectory, item.Date.ToString("yyyy-MM-dd"), videoFile.FileName);
                    try
                    {
                        File.Move(videoFile.FileFullPath, destFile);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"移动文件失败：{videoFile.FileFullPath} => {destFile}");
                    }
                }
            }

            DeleteExpiredFiles();
        }

        /// <summary>
        /// 删除过期文件
        /// </summary>
        private void DeleteExpiredFiles()
        {
            var directories = new DirectoryInfo(_configOption.BaseDirectory).GetDirectories();
            foreach (var dir in directories)
            {
                var date = Convert.ToDateTime(dir.Name);
                if (date < DateTime.Today.AddDays(-_configOption.KeepDays))
                {
                    Directory.Delete(dir.FullName, true);
                }
            }
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="rootDir">根目录</param>
        /// <param name="dates"></param>
        private void CreateFolder(string rootDir, List<DateOnly> dates)
        {
            foreach (var item in dates)
            {
                var dir = Path.Combine(rootDir, item.ToString("yyyy-MM-dd"));
                Directory.CreateDirectory(dir);
            }
        }


        /// <summary>
        /// 获取下级目录，添加异常处理，异常时返回空数组
        /// </summary>
        /// <param name="directoryFullPath"></param>
        /// <returns></returns>
        private List<MergeDto> GetRootFiles(string directoryFullPath)
        {
            var list = new List<MergeDto>();
            try
            {
                var files = new DirectoryInfo(directoryFullPath).GetFiles(_searchPattern);
                foreach (var item in files)
                {
                    var fileName = item.Name;
                    var arr = fileName.Split("_");
                    var dt = DateOnly.ParseExact(arr[2].Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                    list.Add(new MergeDto { VideoType = arr[0], FileName = fileName, Date = dt, FileFullPath = item.FullName });
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"获取文件失败");
            }

            return list;
        }
    }
}