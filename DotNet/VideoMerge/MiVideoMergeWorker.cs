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
    public class XiaomiOutdoorCameraMergeWorker : QuartzBackgroundWorkerBase
    {
        public class MergeDto
        {
            public string VideoType { get; set; }

            public string FileName { get; set; }

            public DateOnly Date { get; set; }
        }


        /// <summary>
        /// 搜索模式（指定后缀或*）
        /// </summary>
        private readonly string _searchPattern;

        private readonly VideoMergeConfigOption _configOption;

        public XiaomiOutdoorCameraMergeWorker(IOptions<VideoMergeConfigOption> configureOptions)
        {
            JobDetail = JobBuilder.Create<XiaomiOutdoorCameraMergeWorker>().WithIdentity(nameof(XiaomiOutdoorCameraMergeWorker)).Build();
            Trigger = TriggerBuilder.Create().WithIdentity(nameof(XiaomiOutdoorCameraMergeWorker))
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
                Logger.LogError($"视频存储目录：{_configOption.BaseDirectory} 不存在，无法进行视频转换操作");
                return;
            }


            // 获取根目录下的文件
            var rootFiles = GetRootFiles(_configOption.BaseDirectory);

            var dates = rootFiles.Select(t => t.Date).Distinct().ToList();
            var lastKeepDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-_configOption.KeepDays);
            var toBeDeleted = rootFiles.Where(t => t.Date < lastKeepDate).ToList();
            foreach (var item in toBeDeleted)
            {
                try
                {
                    File.Delete(item.FileName);
                }
                catch (Exception e)
                {
                    Logger.LogError($"删除文件失败：{item.FileName}", e);
                }
            }

            rootFiles = rootFiles.Except(toBeDeleted).ToList();
            CreateFolder(_configOption.BaseDirectory, dates);
            var groupList = rootFiles
                .Where(t => t.Date < DateOnly.FromDateTime(DateTime.Today))
                .GroupBy(t => new { t.Date, t.VideoType })
                .Select(t => new
                {
                    t.Key.Date,
                    t.Key.VideoType,
                    files = t.ToList()
                })
                .ToList();
            foreach (var item in groupList)
            {
                var manifestFile = Path.Combine(_configOption.BaseDirectory, $"{item.Date.ToString("yyyyMMdd")}_{item.VideoType}.txt");
                var outputFile = Path.Combine(_configOption.BaseDirectory, item.Date.ToString("yyyy-MM"), $"{item.Date.ToString("yyyyMMdd")}_{item.VideoType}.mp4");
                if (File.Exists(outputFile))
                {
                    if (File.Exists(manifestFile))
                    {
                        File.Exists(manifestFile);
                    }
                    continue;
                }

                var videoFiles = item.files.OrderBy(t => t.FileName).Select(t => t.FileName).ToList();
                await WriteVideoFileList(manifestFile, videoFiles);
                await ExecuteVideoMergeAsync(manifestFile, outputFile);
            }
        }

        private void CreateFolder(string configOptionBaseDirectory, List<DateOnly> dates)
        {
            foreach (var item in dates)
            {
                var dir = Path.Combine(configOptionBaseDirectory, item.ToString("yyyy-MM"));
                Directory.CreateDirectory(dir);
            }
        }


        /// <summary>
        /// 创建视频转换清单文件
        /// </summary>
        /// <param name="convertFile">清单文件</param>
        /// <param name="videoFiles">清单中的文件列表</param>
        private async Task WriteVideoFileList(string convertFile, List<string> videoFiles)
        {
            // 将需要合并的文件，记录在以日期命名的文件中，后期ffmpeg合并命令使用
            using (StreamWriter writer = new StreamWriter(convertFile))
            {
                foreach (var file in videoFiles)
                {
                    var videoFileName = new FileInfo(file).Name;
                    await writer.WriteLineAsync($"file {videoFileName}");
                }
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
                    list.Add(new MergeDto { VideoType = arr[0], FileName = fileName, Date = dt });
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"获取文件失败");
            }

            return list;
        }

        /// <summary>
        /// 执行视频合并
        /// </summary>
        /// <param name="convertFile">清单文件</param>
        /// <param name="outputFile">合并后的新文件</param>
        private async Task ExecuteVideoMergeAsync(string convertFile, string outputFile)
        {
            try
            {
                var command = string.Empty;
                var process = new Process();
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true; // 设置为true，执行过程中不在控制台显示，同时，结束当前程序时，无法立即结束ffmpeg转换程序
                // process.StartInfo.UseShellExecute = false;
                if (OperatingSystem.IsLinux())
                {
                    command = $"ffmpeg -safe 0 -f concat -i {convertFile} -c:v copy -c:a aac {outputFile} > /dev/null 2>&1";
                    process.StartInfo.FileName = "/bin/bash";
                }
                else if (OperatingSystem.IsWindows())
                {
                    command = $"ffmpeg -safe 0 -f concat -i {convertFile} -c:v copy -c:a aac {outputFile}";
                    process.StartInfo.FileName = "cmd.exe";
                }

                Logger.LogDebug($"视频合并命令：{command}");
                process.Start();
                process.StandardInput.WriteLine(command);
                process.StandardInput.Flush();
                process.StandardInput.Close();
                await process.WaitForExitAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"发生异常{e.Message}");
            }
        }
    }
}