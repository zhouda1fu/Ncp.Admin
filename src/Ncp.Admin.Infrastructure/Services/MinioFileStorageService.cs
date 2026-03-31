using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Ncp.Admin.Infrastructure.Services;

/// <summary>
/// MinIO 对象存储实现，与本地存储使用相同的 key 格式（yyyyMMdd/guid.ext）。
/// </summary>
public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly bool _ensureBucketOnFirstUse;
    private bool _bucketEnsured;

    public MinioFileStorageService(IOptions<MinioFileStorageOptions> options)
    {
        var opt = options.Value;
        if (string.IsNullOrWhiteSpace(opt.Endpoint))
            throw new InvalidOperationException("FileStorage:MinIO:Endpoint is required.");
        if (string.IsNullOrWhiteSpace(opt.Bucket))
            throw new InvalidOperationException("FileStorage:MinIO:Bucket is required.");

        _bucket = opt.Bucket;
        _ensureBucketOnFirstUse = true;
        _bucketEnsured = false;

        var endpoint = NormalizeEndpoint(opt.Endpoint);
        var builder = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(opt.AccessKey ?? "", opt.SecretKey ?? "");
        if (opt.UseSSL)
            builder = builder.WithSSL();
        _client = builder.Build();
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        var s = endpoint.Trim();
        if (s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return s["https://".Length..].TrimEnd('/');
        if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return s["http://".Length..].TrimEnd('/');
        return s;
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        if (_bucketEnsured)
            return;
        var exists = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_bucket),
            cancellationToken).ConfigureAwait(false);
        if (!exists)
        {
            await _client.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_bucket),
                cancellationToken).ConfigureAwait(false);
        }
        _bucketEnsured = true;
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        if (_ensureBucketOnFirstUse)
            await EnsureBucketAsync(cancellationToken).ConfigureAwait(false);

        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrEmpty(safeName))
            safeName = "file";
        var ext = Path.GetExtension(safeName);
        var subDir = DateTime.UtcNow.ToString("yyyyMMdd");
        var uniqueName = $"{Guid.NewGuid():N}{ext}";
        var objectKey = $"{subDir}/{uniqueName}".Replace('\\', '/');

        long size;
        Stream uploadStream = stream;
        MemoryStream? copy = null;
        if (stream.CanSeek && stream.Length >= 0)
        {
            size = stream.Length;
            if (stream.Position != 0)
                stream.Position = 0;
        }
        else
        {
            copy = new MemoryStream();
            await stream.CopyToAsync(copy, cancellationToken).ConfigureAwait(false);
            copy.Position = 0;
            size = copy.Length;
            uploadStream = copy;
        }

        var contentType = GetContentType(Path.GetExtension(safeName).ToLowerInvariant());
        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey)
            .WithStreamData(uploadStream)
            .WithObjectSize(size)
            .WithContentType(contentType);
        await _client.PutObjectAsync(args, cancellationToken).ConfigureAwait(false);
        if (copy != null)
            await copy.DisposeAsync().ConfigureAwait(false);
        return objectKey;
    }

    /// <inheritdoc />
    public async Task<Stream?> DownloadAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
            return null;

        try
        {
            var ms = new MemoryStream();
            await _client.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(fileKey)
                    .WithCallbackStream(async (s, ct) => { await s.CopyToAsync(ms, ct).ConfigureAwait(false); }),
                cancellationToken).ConfigureAwait(false);
            ms.Position = 0;
            return ms;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
            return;
        try
        {
            await _client.RemoveObjectAsync(
                new RemoveObjectArgs().WithBucket(_bucket).WithObject(fileKey),
                cancellationToken).ConfigureAwait(false);
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            // 已不存在，视为删除成功
        }
    }

    private static string GetContentType(string ext)
    {
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
