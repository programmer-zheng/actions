using System.ComponentModel.DataAnnotations;

namespace VideoMerge
{
    public class VideoMergeConfigOption
    {
        public const string VideoMergeConfigOptionName = "VideoMerge";

        /// <summary>
        /// 基础路径
        /// </summary>
        [Required]
        public string BaseDirectory { get; set; }

        /// <summary>
        /// 合并周期（单位分钟）
        /// </summary>
        [Required]
        public int MergeCycle { get; set; }

        /// <summary>
        /// 视频后缀
        /// </summary>
        [Required]
        public string VideoSuffix { get; set; }

        /// <summary>
        /// 保留天数（单位天）
        /// </summary>
        [Required]
        public int KeepDays { get; set; }

        /// <summary>
        /// 移动视频文件周期（单位分钟）
        /// </summary>
        [Required]
        public int MoveMinutes { get; set; }
    }
}