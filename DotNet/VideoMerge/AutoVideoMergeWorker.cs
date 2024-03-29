﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace VideoMerge
{
    public class AutoVideoMergeWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private readonly IConfiguration _configuration;


        private readonly string _searchPattern;

        private readonly VideoMergeConfigOption _configOption;

        public AutoVideoMergeWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, IOptions<VideoMergeConfigOption> configureOptions)
            : base(timer, serviceScopeFactory)
        {
            timer.Period = 1000 * 3;
            timer.Start();
            _configuration = configuration;
            _configOption = configureOptions.Value;
            _searchPattern = $"*{_configOption.VideoSuffix}";
        }


        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            // 第一次启动后，修改同步周期
            Timer.Period = 1000 * 60 * _configOption.MergeCycle;

            if (Directory.Exists(_configOption.BaseDirectory))
            {
                // 获取当前日期和小时
                var currentDateHour = DateTime.Now.ToString("yyyyMMddHH");
                Logger.LogInformation($"当前时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}，对应目录：{currentDateHour}");

                var dayRegex = new Regex(@"^\d{8,10}$");

                var lastModifyTime = new DirectoryInfo(_configOption.BaseDirectory).LastWriteTime;
                // 获取根目录下的文件夹
                var dateDirectories = Directory.GetDirectories(_configOption.BaseDirectory);
                foreach (var dir in dateDirectories)
                {
                    // 获取目录名称，后续用作合并视频文件名
                    var directoryName = new DirectoryInfo(dir).Name;
                    Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 操作目录:{directoryName}");

                    // 排除当前时间的目录（当前时间视频不完整，不足一小时）、文件夹名格式不正确的（日期+小时）
                    // 合并后的视频存放于以日期命名的文件夹中，按小时区分
                    if (directoryName == currentDateHour)
                    {
                        continue;
                    }

                    if (!dayRegex.IsMatch(directoryName))
                    {
                        Logger.LogInformation($"跳过目录：{directoryName}");
                        continue;
                    }

                    var year = Convert.ToInt32(directoryName.Substring(0, 4));
                    var month = Convert.ToInt32(directoryName.Substring(4, 2));
                    var day = Convert.ToInt32(directoryName.Substring(6, 2));

                    if (directoryName.Length == 8)
                    {
                        // 8位长度（yyyyMMdd)，判断是否超过保留时长，若超过时长，删除目录并跳过
                        if (new DateTime(year, month, day) < DateTime.Today.AddDays(-_configOption.KeepDays))
                        {
                            Directory.Delete(dir, true);
                            Logger.LogInformation($"超出保留时限，删除目录：{directoryName}");
                        }

                        continue;
                    }

                    var hour = Convert.ToInt32(directoryName.Substring(8, 2));
                    var dirMaxTime = new DateTime(year, month, day, hour, 59, 59);
                    // 摄像头往NAS里面同步需要时间，由于未知原因，导致同步过来的视频，有可能不足60个（网络原因、摄像头重启、存储卡意外损坏等）
                    // 例：2023091813 文件夹中，只有50个视频文件，只有当最后修改时间（相当于最后同步时间）在 2023-9-18 13:59:59 之后，才同步 2023091813 文件夹
                    if (!(dirMaxTime < lastModifyTime))
                    {
                        continue;
                    }

                    //截取日期部分
                    var videoDate = directoryName.Substring(0, 8);
                    var videoDateDirectoryPath = Path.Combine(_configOption.BaseDirectory, videoDate);
                    if (!Directory.Exists(videoDateDirectoryPath))
                    {
                        Directory.CreateDirectory(videoDateDirectoryPath);
                    }

                    // ffmpeg合并视频文件
                    var convertFile = Path.Combine(_configOption.BaseDirectory, directoryName, $"{directoryName}.txt");
                    // 合并后的视频输出文件
                    var outputFile = Path.Combine(videoDateDirectoryPath, $"{directoryName}.mp4");
                    // 若已转换过，删除原目录，避免重复执行合并操作，之所以不在转换后立即执行，是避免转换有错误
                    if (File.Exists(outputFile))
                    {
                        Logger.LogInformation($"已存在合并后的视频：{outputFile}，删除源视频目录：{directoryName}");
                        Directory.Delete(dir, true);
                        continue;
                    }

                    // 获取文件夹的视频文件
                    var videoFiles = Directory.GetFiles(dir, _searchPattern);

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
                        var cmdOutput = await process.StandardOutput.ReadToEndAsync();
                        Logger.LogDebug(cmdOutput);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"发生异常{e.Message}");
                    }
                }
            }
            else
            {
                Logger.LogError($"视频存储目录：{_configOption.BaseDirectory} 不存在，无法进行视频转换操作");
            }
        }
    }
}