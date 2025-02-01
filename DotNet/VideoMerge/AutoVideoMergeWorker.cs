using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace VideoMerge
{
    public class AutoVideoMergeWorker : AsyncPeriodicBackgroundWorkerBase
    {
        /// <summary>
        /// 搜索模式（指定后缀或*）
        /// </summary>
        private readonly string _searchPattern;

        private readonly VideoMergeConfigOption _configOption;

        public AutoVideoMergeWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory, IOptions<VideoMergeConfigOption> configureOptions)
            : base(timer, serviceScopeFactory)
        {
            timer.Period = 1000 * 3;
            timer.Start();
            _configOption = configureOptions.Value;
            _searchPattern = $"*{_configOption.VideoSuffix}";
        }


        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            // 第一次启动后，修改同步周期
            Timer.Period = 1000 * 60 * _configOption.MergeCycle;

            if (!Directory.Exists(_configOption.BaseDirectory))
            {
                Logger.LogError($"视频存储目录：{_configOption.BaseDirectory} 不存在，无法进行视频转换操作");
                return;
            }

            // 获取当前日期和小时
            var currentDateHour = DateTime.Now.ToString("yyyyMMddHH");

            var lastModifyTime = new DirectoryInfo(_configOption.BaseDirectory).LastWriteTime;
            // 获取根目录下的文件夹

            var dateDirectories = GetChildrenDirectories(_configOption.BaseDirectory);

            foreach (var dir in dateDirectories)
            {
                await ProcessDirectory(dir, currentDateHour, lastModifyTime);
            }
        }

        /// <summary>
        /// 处理文件夹
        /// </summary>
        /// <param name="dir">目录（完整路径）</param>
        /// <param name="currentDateHour">当前日期小时</param>
        /// <param name="lastModifyTime">根目录最后修改时间</param>
        private async Task ProcessDirectory(string dir, string currentDateHour, DateTime lastModifyTime)
        {
            // 获取目录名称，后续用作合并视频文件名
            var directoryName = GetDirectoryName(dir);

            Logger.LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 操作目录:{directoryName}");

            if (ShouldSkipDirectory(directoryName, currentDateHour))
                return;

            if (IsOldDirectory(directoryName))
            {
                Directory.Delete(dir, true);
                Logger.LogInformation($"超出保留时限，删除目录：{directoryName}");
                return;
            }

            if (IsReadyForProcessing(directoryName, lastModifyTime))
            {
                await ProcessVideoFiles(dir);
            }
        }

        private string GetDirectoryName(string dir)
        {
            return new DirectoryInfo(dir).Name;
        }

        /// <summary>
        /// 是否跳过文件夹
        /// </summary>
        /// <param name="directoryName">文件夹名称</param>
        /// <param name="currentDateHour">当前时间，日期+小时（yyyyMMddHH）</param>
        /// <returns></returns>
        private bool ShouldSkipDirectory(string directoryName, string currentDateHour)
        {
            // 排除当前时间的目录（当前时间视频不完整，不足一小时）、文件夹名格式不正确的（日期+小时）
            // 合并后的视频存放于以日期命名的文件夹中，按小时区分
            if (directoryName == currentDateHour || !Regex.IsMatch(directoryName, @"^\d{8,10}$"))
            {
                Logger.LogInformation($"跳过目录：{directoryName}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否过期文件夹（超出保留时长限制）
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        private bool IsOldDirectory(string directoryName)
        {
            if (directoryName.Length == 8)
            {
                // 提取文件夹中的日期部分
                var year = Convert.ToInt32(directoryName.Substring(0, 4));
                var month = Convert.ToInt32(directoryName.Substring(4, 2));
                var day = Convert.ToInt32(directoryName.Substring(6, 2));
                if (new DateTime(year, month, day) < DateTime.Today.AddDays(-_configOption.KeepDays))
                {
                    // 判断文件夹是否超出配置文件中定义的时限 
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// 判断指定文件夹里的文件是否可以处理
        /// </summary>
        /// <param name="directoryName">文件夹名称</param>
        /// <param name="lastModifyTime">根目录最后修改时间</param>
        /// <returns></returns>
        private bool IsReadyForProcessing(string directoryName, DateTime lastModifyTime)
        {
            var year = Convert.ToInt32(directoryName.Substring(0, 4));
            var month = Convert.ToInt32(directoryName.Substring(4, 2));
            var day = Convert.ToInt32(directoryName.Substring(6, 2));
            var hour = Convert.ToInt32(directoryName.Substring(8, 2));
            var dirMaxTime = new DateTime(year, month, day, hour, 59, 59);

            // 摄像头往NAS里面同步需要时间，由于未知原因，导致同步过来的视频，有可能不足60个（网络原因、摄像头重启、存储卡意外损坏等）
            // 例：2023091813 文件夹中，只有50个视频文件，只有当最后修改时间（相当于最后同步时间）在 2023-9-18 13:59:59 之后，才处理 2023091813 文件夹
            return dirMaxTime < lastModifyTime;
        }

        /// <summary>
        /// 处理视频文件
        /// </summary>
        /// <param name="dir">需要转换的视频文件目录</param>
        private async Task ProcessVideoFiles(string dir)
        {
            var directoryName = GetDirectoryName(dir);
            var videoDate = directoryName.Substring(0, 8); // 截取文件夹名称中日期部分
            var videoDateDirectoryPath = Path.Combine(_configOption.BaseDirectory, videoDate);
            if (!Directory.Exists(videoDateDirectoryPath))
            {
                Directory.CreateDirectory(videoDateDirectoryPath);
            }

            var convertFile = Path.Combine(_configOption.BaseDirectory, directoryName, $"{directoryName}.txt"); // 清单文件
            var outputFile = Path.Combine(videoDateDirectoryPath, $"{directoryName}.mp4"); //合并后的新文件 

            if (File.Exists(outputFile))
            {
                // 若已转换过，删除原目录，避免重复执行合并操作，之所以不在转换后立即执行，是避免转换有错误
                Logger.LogInformation($"已存在合并后的视频：{outputFile}，删除源视频目录：{directoryName}");
                Directory.Delete(dir, true);
                return;
            }

            WriteVideoFileList(dir, convertFile);
            await ExecuteVideoMergeAsync(convertFile, outputFile);
        }

        /// <summary>
        /// 创建视频转换清单文件
        /// </summary>
        /// <param name="dir">需要转换的视频文件目录</param>
        /// <param name="convertFile">清单文件</param>
        private void WriteVideoFileList(string dir, string convertFile)
        {
            var videoFiles = Directory.GetFiles(dir, _searchPattern);

            // 将需要合并的文件，记录在以日期命名的文件中，后期ffmpeg合并命令使用
            using (StreamWriter writer = new StreamWriter(convertFile))
            {
                foreach (var file in videoFiles)
                {
                    var videoFileName = new FileInfo(file).Name;
                    writer.WriteLine($"file {videoFileName}");
                }
            }
        }

        /// <summary>
        /// 获取下级目录，添加异常处理，异常时返回空数组
        /// </summary>
        /// <param name="directoryFullPath"></param>
        /// <returns></returns>
        private string[] GetChildrenDirectories(string directoryFullPath)
        {
            try
            {
                return Directory.GetDirectories(directoryFullPath);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"获取子级目录失败");
            }

            return [];
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