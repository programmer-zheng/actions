## 下载ffmpeg
[下载ffmpeg](https://ffmpeg.org/download.html)
           
## Windows
### 设置环境变量
将 `ffmpeg.exe` 所属目录添加到环境变量 `Path` 中

### 转换
``` bash
# -i 指定文件名
ffmpeg -safe 0 -f concat -i C:\micamera\2023091417\2023091417.txt -c:v copy -c:a aac C:\micamera\2023091417.mp4

```