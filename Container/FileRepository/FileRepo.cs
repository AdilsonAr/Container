using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using Amazon.Runtime;
using Container.Secrets;

namespace Container.FileRepository
{
    public class FileRepo
    {
        public async Task<string> testListBucketsAsync()
        {
            string r = "";
            var s3Client = new AmazonS3Client(S3.awsAccessKeyId,
                S3.awsSecretAccessKey, S3.regionEndpoint);
            var listResponse = await s3Client.ListBucketsAsync();
            foreach (S3Bucket b in listResponse.Buckets)
            {
                r+=b.BucketName;
            }           
            return r;
        }
    }
}