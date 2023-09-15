// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;

public class Program
{
    static void Main(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true);
        var config = configurationBuilder.Build();
        var videoBaseDirectory = config.GetValue<string>("BaseDirectory");
        if (Directory.Exists(videoBaseDirectory))
        {
            var dateDirectories = Directory.GetDirectories(videoBaseDirectory);
            foreach (var dir in dateDirectories)
            {
                // 获取目录名称，后续用作合并视频文件名
                var directoryName = new DirectoryInfo(dir).Name;
                var videoFiles = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
                var convertFile = Path.Combine(videoBaseDirectory, directoryName, $"{directoryName}.txt");
                var outputFile = Path.Combine(videoBaseDirectory, $"{directoryName}.mp4");
                using (StreamWriter writer = new StreamWriter(convertFile))
                {
                    foreach (var file in videoFiles)
                    {
                        var videoFileName = new FileInfo(file).Name;
                        writer.WriteLine($"file {videoFileName}");
                    }
                }

                // 拼接转换命令
                Console.WriteLine($"ffmpeg -safe 0 -f concat -i {convertFile} -c:v copy -c:a aac {outputFile}");
            }
        }
    }
}