using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>Minio 对象存储</summary>
    public class MinioService
    {
        private readonly IMinioClient _client;
        private readonly MinioSettings _settings;

        public MinioService(IMinioClient client, IOptions<MinioSettings> options)
        {
            _client = client;
            _settings = options.Value;
        }

        /// <summary>上传文件，按日期归档并生成唯一文件名，返回对象路径</summary>
        public async Task<string> UploadFile(string objectName, Stream data, string contentType = "application/octet-stream")
        {
            var now = DateTime.Now;
            var ext = Path.GetExtension(objectName);
            var random = Guid.NewGuid().ToString("N")[..8];
            var newObjectName = $"{now:yyyyMMdd}/{now:yyyyMMddHHmmssfff}_{random}{ext}";

            await _client.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_settings.DefaultBucket)
                .WithObject(newObjectName)
                .WithStreamData(data)
                .WithObjectSize(data.Length)
                .WithContentType(contentType));

            return $"/{newObjectName}";
        }

        /// <summary>下载为内存流</summary>
        public async Task<MemoryStream> DownloadFile(string objectName)
        {
            var ms = new MemoryStream();

            await _client.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_settings.DefaultBucket)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(ms)));

            ms.Position = 0;
            return ms;
        }

        /// <summary>下载为字节数组</summary>
        public async Task<byte[]> DownloadBytesAsync(string objectName)
        {
            using var ms = await DownloadFile(objectName);
            return ms.ToArray();
        }

        /// <summary>下载到本地文件</summary>
        public async Task DownloadToFileAsync(string objectName, string localFilePath)
        {
            await _client.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_settings.DefaultBucket)
                .WithObject(objectName)
                .WithFile(localFilePath));
        }

        /// <summary>删除文件</summary>
        public async Task DeleteFile(string objectName)
        {
            await _client.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_settings.DefaultBucket)
                .WithObject(objectName));
        }

        /// <summary>批量删除文件</summary>
        public async Task DeleteManyFile(IEnumerable<string> objectNames)
        {
            foreach (var name in objectNames)
                await DeleteFile(name);
        }
    }
}
