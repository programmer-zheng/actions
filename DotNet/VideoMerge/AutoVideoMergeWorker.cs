using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace VideoMerge
{
    public class AutoVideoMergeWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private readonly IConfiguration _configuration;

        private readonly string VideoBaseDirectory;

        public AutoVideoMergeWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration) : base(timer, serviceScopeFactory)
        {
            timer.Period = 1000 * 5;
            timer.Start();
            _configuration = configuration;
            VideoBaseDirectory = _configuration.GetValue<string>("BaseDirectory");
        }


        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            Timer.Period = 1000 * 60 * 60;

            if (Directory.Exists(VideoBaseDirectory))
            {
                // 获取当前日期和小时
                var currentDateHour = DateTime.Now.ToString("yyyyMMddHH");
                // 获取根目录下的文件夹
                var dateDirectories = Directory.GetDirectories(VideoBaseDirectory);
                foreach (var dir in dateDirectories)
                {
                    Logger.LogInformation($"操作目录{dir}");
                    // 获取目录名称，后续用作合并视频文件名
                    var directoryName = new DirectoryInfo(dir).Name;
                    // 排除当前时间的目录（当前时间视频不完整，不足一小时）、文件夹名格式不正确的（日期+小时）
                    // 合并后的视频存放于以日期命名的文件夹中，按小时区分
                    if (directoryName == currentDateHour || !Regex.IsMatch(directoryName, @"^\d{10}$"))
                    {
                        continue;
                    }

                    // ffmpeg合并视频文件
                    var convertFile = Path.Combine(VideoBaseDirectory, directoryName, $"{directoryName}.txt");
                    // 合并后的视频输出文件
                    var outputFile = Path.Combine(VideoBaseDirectory, $"{directoryName}.mp4");
                    // 若已转换过，删除原目录，避免重复执行合并操作，之所以不在转换后立即执行，是避免转换有错误
                    if (File.Exists(outputFile))
                    {
                        Logger.LogInformation($"已存在{outputFile}，删除{dir}");
                        Directory.Delete(dir, true);
                        continue;
                    }

                    // 获取文件夹的视频文件
                    var videoFiles = Directory.GetFiles(dir);
                    try
                    {
                        // 将需要合并的文件，记录在以日期命名的文件中，后期ffmpeg合并命令使用
                        using (StreamWriter writer = new StreamWriter(convertFile))
                        {
                            foreach (var file in videoFiles)
                            {
                                var videoFileName = new FileInfo(file).Name;
                                writer.WriteLine($"file {videoFileName}");
                            }
                        }

                        // 拼接转换命令
                        var command = string.Empty;
                        if (OperatingSystem.IsLinux())
                        {
                        }
                        else if (OperatingSystem.IsWindows())
                        {
                            command = $"ffmpeg -safe 0 -f concat -i {convertFile} -c:v copy -c:a aac {outputFile}";
                            var cmd = new Process();
                            cmd.StartInfo.FileName = "cmd.exe";
                            cmd.StartInfo.RedirectStandardInput = true;
                            cmd.StartInfo.RedirectStandardOutput = true;
                            cmd.StartInfo.CreateNoWindow = true; // 设置为true，执行过程中不在控制台显示，同时，结束当前程序时，无法立即结束ffmpeg转换程序
                            // cmd.StartInfo.UseShellExecute = false;
                            cmd.Start();

                            cmd.StandardInput.WriteLine(command);
                            cmd.StandardInput.Flush();
                            cmd.StandardInput.Close();
                            await cmd.WaitForExitAsync();
                            var cmdOutput = await cmd.StandardOutput.ReadToEndAsync();
                            Logger.LogDebug(cmdOutput);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"发生异常{e.Message}");
                    }
                }
            }
            else
            {
                Logger.LogError($"视频存储目录：{VideoBaseDirectory} 不存在，无法进行视频转换操作");
            }
        }
    }
}