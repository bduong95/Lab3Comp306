using Amazon.S3;
using Amazon.S3.Transfer;
using System.IO;
using System.Threading.Tasks;

namespace MovieStreamingApp.Services
{
    public class S3Service
    {
        private readonly string bucketName = "movie-streaming-app-bucket";
        private readonly IAmazonS3 s3Client;

        public S3Service()
        {
            // Temporarily setting credentials in code for local testing
            s3Client = new AmazonS3Client("AKIAVPEYWTU5EFFNO5XM", "Ys/3uqee62UYyVQhujOfY566TfJ+rFtlMuZKQ9Pq", Amazon.RegionEndpoint.USEast1);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileName,
                BucketName = bucketName,
                CannedACL = S3CannedACL.Private
            };

            var fileTransferUtility = new TransferUtility(s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return $"https://{bucketName}.s3.amazonaws.com/{fileName}";
        }
    }
}
