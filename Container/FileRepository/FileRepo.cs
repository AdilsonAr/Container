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
using Amazon.S3.Transfer;
using System.Configuration;
using System.IO;
using System.Web.Mvc;

namespace Container.FileRepository
{
    public class FileRepo
    {
        public TransferUtility getUtility()
        {
            var s3Client = new AmazonS3Client(S3.awsAccessKeyId,
                S3.awsSecretAccessKey, S3.regionEndpoint);
            return new TransferUtility(s3Client);           
        }

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

        public async Task<bool> upload(HttpPostedFileBase file, string path, string bucket, string nombre)
        {
            file.SaveAs(path);
            var s3Client = new AmazonS3Client(S3.awsAccessKeyId, S3.awsSecretAccessKey, S3.regionEndpoint);
            bool success = false;
            var fileTransferUtility = new TransferUtility(s3Client);
            try
            {
                if (file.ContentLength > 0)
                {
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = bucket,
                        FilePath = path,
                        PartSize = 256,//256 Bytes 
                        Key = nombre,
                        CannedACL = S3CannedACL.Private
                    };
                    //fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                   
                    fileTransferUtility.Upload(fileTransferUtilityRequest);
                    fileTransferUtility.Dispose();
                }
                success = true;
            }

            catch (AmazonS3Exception amazonS3Exception)
            {
               
            }
            return success;
        }
    }
}